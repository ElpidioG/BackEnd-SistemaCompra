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
                .Include(d => d.Articulo) // Asegúrate de que la relación está configurada
                .Include(d => d.UnidadMedida) // Asegúrate de que la relación está configurada
                .Select(d => new
                {
                    d.Id,
                    Articulo = d.Articulo.Descripcion, // Devuelve el nombre del artículo
                    d.Cantidad,
                    UnidadMedida = d.UnidadMedida.Descripcion, // Devuelve el nombre de la unidad de medida
                    d.CostoUnitario,
                    d.CostoTotal
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // POST: api/DetalleOrdenCompras
        [HttpPost]
        public async Task<ActionResult> PostDetalleOrdenCompra(List<DetalleOrdenCompra> detallesOrdenCompra)
        {
            if (detallesOrdenCompra == null || detallesOrdenCompra.Count == 0)
            {
                return BadRequest(new { mensaje = "Los datos enviados son nulos o vacíos." });
            }

            foreach (var detalleOrdenCompra in detallesOrdenCompra)
            {
                var errores = ValidarDetalleOrdenCompra(detalleOrdenCompra);
                if (errores.Any())
                {
                    return BadRequest(new { errores });
                }

                var articulo = await _context.Tbl_Articulos.FindAsync(detalleOrdenCompra.IdArticulo);
                if (articulo == null)
                {
                    return NotFound(new { mensaje = $"El artículo con ID {detalleOrdenCompra.IdArticulo} no existe." });
                }

                var unidadMedida = await _context.Tbl_UnidadesMedidas.FindAsync(detalleOrdenCompra.IdUnidadMedida);
                if (unidadMedida == null)
                {
                    return NotFound(new { mensaje = $"La unidad de medida con ID {detalleOrdenCompra.IdUnidadMedida} no existe." });
                }

                var ordenCompra = await _context.Tbl_OrdenCompra.FindAsync(detalleOrdenCompra.IdOrdenCompra);
                if (ordenCompra == null)
                {
                    return NotFound(new { mensaje = $"La orden de compra con ID {detalleOrdenCompra.IdOrdenCompra} no existe." });
                }

                // Verificar existencia de artículos
                if (articulo.Existencia < detalleOrdenCompra.Cantidad)
                {
                    return BadRequest(new { mensaje = $"No hay suficiente existencia para el artículo {detalleOrdenCompra.IdArticulo}." });
                }

                articulo.Existencia -= detalleOrdenCompra.Cantidad;
                _context.Tbl_Articulos.Update(articulo);

                detalleOrdenCompra.Articulo = articulo;
                detalleOrdenCompra.UnidadMedida = unidadMedida;
                detalleOrdenCompra.OrdenCompra = ordenCompra;

                _context.Tbl_Detalle_OrdenCompra.Add(detalleOrdenCompra);
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Detalles de la orden de compra guardados exitosamente." });
        }
        // DELETE: api/DetalleOrdenCompras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalleOrdenCompra(int id)
        {
            var detalleOrdenCompra = await _context.Tbl_Detalle_OrdenCompra.FindAsync(id);
            if (detalleOrdenCompra == null)
            {
                return NotFound();
            }

            _context.Tbl_Detalle_OrdenCompra.Remove(detalleOrdenCompra);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DetalleOrdenCompraExists(int id)
        {
            return _context.Tbl_Detalle_OrdenCompra.Any(e => e.Id == id);
        }
        private List<string> ValidarDetalleOrdenCompra(DetalleOrdenCompra detalleOrdenCompra)
        {
            var errores = new List<string>();

            if (detalleOrdenCompra.IdUnidadMedida <= 0)
                errores.Add("Debe seleccionar un unidad de medida valida.");

            if (detalleOrdenCompra.IdArticulo <= 0)
                errores.Add("Debe seleccionar un artículo válido.");

            if (detalleOrdenCompra.IdOrdenCompra <= 0)
                errores.Add("El número de orden de compra no es válido.");

            if (detalleOrdenCompra.Cantidad <= 0)
                errores.Add("Debe seleccionar una cantidad válida mayor a 0.");

            if (detalleOrdenCompra.CostoUnitario <= 0)
                errores.Add("Debe ingresar un costo unitario válido mayor a 0.");

            if (detalleOrdenCompra.CostoTotal <= 0)
                errores.Add("Debe ingresar un costo total válido mayor a 0.");

            if (detalleOrdenCompra.CostoTotal != detalleOrdenCompra.Cantidad * detalleOrdenCompra.CostoUnitario)
                errores.Add("El costo total debe ser igual a la cantidad multiplicada por el costo unitario.");

            return errores;
        }
    }
}