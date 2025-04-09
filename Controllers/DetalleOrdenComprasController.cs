using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd_SistemaCompra.Contexts;
using BackEnd_SistemaCompra.Models;

namespace BackEnd_SistemaCompra.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleOrdenComprasController : ControllerBase
    {
        private readonly ConexionDB _context;

        public DetalleOrdenComprasController(ConexionDB context)
        {
            _context = context;
        }

        // GET: api/DetalleOrdenCompras/Orden/5
        [HttpGet("Orden/{idOrdenCompra}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDetallesPorOrden(int idOrdenCompra)
        {
            var detalles = await _context.Tbl_Detalle_OrdenCompra
                .Where(d => d.IdOrdenCompra == idOrdenCompra)
                .Include(d => d.Articulo)
                .Include(d => d.UnidadMedida)
                .Select(d => new
                {
                    d.Id,
                    Articulo = d.Articulo.Descripcion,
                    d.Cantidad,
                    UnidadMedida = d.UnidadMedida.Descripcion,
                    CostoUnitario = d.Articulo.CostoUnitario,
                    CostoTotal = d.Articulo.CostoUnitario * d.Cantidad
                })
                .ToListAsync();

            if (detalles == null || detalles.Count == 0)
            {
                return NotFound();
            }

            return Ok(detalles);
        }
        // PUT: api/DetalleOrdenCompras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalleOrdenCompra(int id, DetalleOrdenCompra detalleOrdenCompra)
        {
            if (id != detalleOrdenCompra.Id)
            {
                return BadRequest();
            }
            Console.WriteLine("Esto es lo que trae: ", detalleOrdenCompra);
            var errores = ValidarDetalleOrdenCompra(detalleOrdenCompra);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }
            _context.Entry(detalleOrdenCompra).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetalleOrdenCompraExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DetalleOrdenCompras
        [HttpPost]
        public async Task<IActionResult> PostDetalleOrdenCompra(List<DetalleOrdenCompra> detalles)
        {
            try
            {
                if (detalles == null || detalles.Count == 0)
                {
                    return BadRequest("No se han proporcionado detalles válidos.");
                }

                int idOrdenCompra = detalles.First().IdOrdenCompra;

                foreach (var detalle in detalles)
                {
                    var articulo = await _context.Tbl_Articulos.FindAsync(detalle.IdArticulo);
                    if (articulo == null)
                    {
                        return BadRequest($"El artículo con ID {detalle.IdArticulo} no existe.");
                    }
                    if (articulo.Existencia < detalle.Cantidad)
                    {
                        return BadRequest($"No hay suficiente existencia para el artículo {articulo.Descripcion}.");
                    }

                    // Actualizar la existencia del artículo
                    articulo.Existencia -= detalle.Cantidad;
                    _context.Tbl_Articulos.Update(articulo);

                    // Agregar el detalle de la orden de compra
                    _context.Tbl_Detalle_OrdenCompra.Add(detalle);
                }

                // Guardar los detalles de la orden de compra antes de calcular el asiento
                await _context.SaveChangesAsync();

                // Ahora calcular el monto total después de guardar los detalles
                decimal montoTotal = await _context.Tbl_Detalle_OrdenCompra
                    .Where(d => d.IdOrdenCompra == idOrdenCompra)
                    .SumAsync(d => d.CostoTotal);

                // Insertar el asiento contable después de confirmar la inserción
                var asiento = new AsientoContable
                {
                    IdAsiento = 80,
                    Descripcion = "Compra registrada - Orden " + idOrdenCompra,
                    IdTipoInventario = 1,
                    CuentaContable = "5",
                    TipoMovimiento = "DB",
                    FechaAsiento = DateTime.Now,
                    Monto = montoTotal,
                    Estado = true
                };

                _context.Tbl_AsientosContables.Add(asiento);

                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(PostDetalleOrdenCompra), detalles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar detalles: {ex.Message}");
                return StatusCode(500, "Error interno del servidor.");
            }
        }
        // DELETE: api/DetalleOrdenCompras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalleOrdenCompra(int id)
        {
            var detalleOrdenCompra = await _context.Tbl_Detalle_OrdenCompra
                .Include(d => d.Articulo)  // Incluir la relación con el artículo
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detalleOrdenCompra == null)
            {
                return NotFound(new { mensaje = "Detalle de la orden de compra no encontrado." });
            }

      
            // Revertir la cantidad al artículo, sumando de vuelta lo que se había restado
            detalleOrdenCompra.Articulo.Existencia += detalleOrdenCompra.Cantidad;
            _context.Tbl_Articulos.Update(detalleOrdenCompra.Articulo);

            // Eliminar el detalle de la orden de compra
            _context.Tbl_Detalle_OrdenCompra.Remove(detalleOrdenCompra);

            await _context.SaveChangesAsync();

            return NoContent();  // Confirmar la eliminación exitosa
        }

        private bool DetalleOrdenCompraExists(int id)
        {
            return _context.Tbl_Detalle_OrdenCompra.Any(e => e.Id == id);
        }
        private List<string> ValidarDetalleOrdenCompra(DetalleOrdenCompra detalle)
        {
            var errores = new List<string>();

            if (detalle.IdUnidadMedida <= 0)
                errores.Add("Debe seleccionar una unidad de medida válida.");

            if (detalle.IdArticulo <= 0)
                errores.Add("Debe seleccionar un artículo válido.");

            if (detalle.IdOrdenCompra <= 0)
                errores.Add("El número de orden de compra no es válido.");

            if (detalle.Cantidad <= 0)
                errores.Add("Debe seleccionar una cantidad válida mayor a 0.");

            return errores;
        }
    }
}