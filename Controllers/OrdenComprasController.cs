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
    public class OrdenComprasController : ControllerBase
    {
        private readonly ConexionDB _context;

        public OrdenComprasController(ConexionDB context)
        {
            _context = context;
        }

        // GET: api/OrdenCompras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTbl_OrdenCompra()
        {
            var ordenes = await _context.Tbl_OrdenCompra
                .Include(o => o.Proveedor)
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    ProveedorNombre = o.Proveedor != null ? o.Proveedor.NombreComercial : "Sin proveedor",
                    o.Estado
                })
                .ToListAsync();

            return Ok(ordenes);
        }

        // GET: api/OrdenCompras/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdenCompraPorId(int id)
        {
            var ordenCompra = await _context.Tbl_OrdenCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Articulo)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.UnidadMedida)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenCompra == null)
            {
                return NotFound();
            }

            var ordenCompraDTO = new
            {
                Id = ordenCompra.Id,
                Fecha = ordenCompra.Fecha,
                Proveedor = new
                {
                    Id = ordenCompra.idProveedor,
                    Nombre = ordenCompra.Proveedor?.NombreComercial ?? "(Sin nombre)"
                },
                Detalles = ordenCompra.Detalles.Select(d => new
                {
                    d.Id,
                    Articulo = new
                    {
                        Id = d.IdArticulo,
                        Nombre = d.Articulo?.Descripcion ?? "(Sin nombre)"
                    },
                    d.Cantidad,
                    UnidadMedida = new
                    {
                        Id = d.IdUnidadMedida,
                        Nombre = d.UnidadMedida?.Descripcion ?? "(Sin nombre)"
                    },
                    PrecioUnitario = d.Articulo.CostoUnitario,
                    PrecioTotal = d.CostoTotal
                })
            };

            return Ok(ordenCompraDTO);
        }

        // PUT: api/OrdenCompras/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrdenCompra(int id, OrdenCompra ordenCompra)
        {
            Console.WriteLine($"ID: {ordenCompra.Id}, Proveedor: {ordenCompra.idProveedor}, Estado: {ordenCompra.Estado}, Fecha: {ordenCompra.Fecha}");

            if (id != ordenCompra.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            var errores = ValidarOrdenCompra(ordenCompra);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }

            _context.Entry(ordenCompra).State = EntityState.Modified;

            // Eliminar el asiento contable si la orden fue inactivada
            if (ordenCompra.Estado == false)
            {
                var asientoContable = await _context.Tbl_AsientosContables
                    .FirstOrDefaultAsync(a => a.Descripcion == "Compra registrada - Orden " + ordenCompra.Id);

                if (asientoContable != null)
                {
                    _context.Tbl_AsientosContables.Remove(asientoContable);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrdenCompraExists(id))
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

        // POST: api/OrdenCompras
        [HttpPost]
        public async Task<ActionResult<OrdenCompra>> PostOrdenCompra(OrdenCompra ordenCompra)
        {
            if (ordenCompra.idProveedor == 0)
            {
                return BadRequest("El proveedor es obligatorio.");
            }

            _context.Tbl_OrdenCompra.Add(ordenCompra);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostOrdenCompra), new { id = ordenCompra.Id }, ordenCompra);
        }

        // DELETE: api/OrdenCompras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrdenCompra(int id)
        {
            var ordenCompra = await _context.Tbl_OrdenCompra
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenCompra == null)
            {
                return NotFound();
            }

            if (ordenCompra.Detalles.Any())
            {
                _context.Tbl_Detalle_OrdenCompra.RemoveRange(ordenCompra.Detalles);
            }

            _context.Tbl_OrdenCompra.Remove(ordenCompra);

            var asientoContable = await _context.Tbl_AsientosContables
                .FirstOrDefaultAsync(a => a.Descripcion == "Compra registrada - Orden " + ordenCompra.Id);

            if (asientoContable != null)
            {
                _context.Tbl_AsientosContables.Remove(asientoContable);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al eliminar la orden de compra y sus detalles.");
            }

            return NoContent();
        }

        private bool OrdenCompraExists(int id)
        {
            return _context.Tbl_OrdenCompra.Any(e => e.Id == id);
        }

        private List<string> ValidarOrdenCompra(OrdenCompra ordenCompra)
        {
            var errores = new List<string>();

            if (ordenCompra.Fecha == default(DateTime) || ordenCompra.Fecha < DateTime.Today)
                errores.Add("Debe ingresar una fecha válida y no anterior a hoy.");

            if (ordenCompra.idProveedor <= 0)
                errores.Add("Debe seleccionar un proveedor válido.");

            if (ordenCompra.Estado == null)
            {
                errores.Add("El estado es obligatorio.");
            }

            return errores;
        }

        [HttpPost("{id}/asiento")]
        public async Task<IActionResult> CrearAsientoContable(int id)
        {
            var ordenCompra = await _context.Tbl_OrdenCompra
                .Include(o => o.Detalles)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenCompra == null)
            {
                return NotFound("Orden de compra no encontrada.");
            }

            // Verificar si ya existe un asiento contable para esta orden
            var asientoContableExistente = await _context.Tbl_AsientosContables
                .FirstOrDefaultAsync(a => a.Descripcion == $"Compra registrada - Orden {ordenCompra.Id} (Débito)");

            if (asientoContableExistente != null)
            {
                return BadRequest("Ya existe un asiento contable para esta orden.");
            }

            // Crear el nuevo asiento contable
            // Débito en la cuenta 80
            var asientoDebito = new AsientoContable
            {
                sistemaAuxiliarId = 7,
                Descripcion = $"Débito de Compra registrada - Orden {ordenCompra.Id}",
                IdTipoInventario = 1, // Ajusta según tu lógica
                CuentaContable = "80",
                TipoMovimiento = "DB",
                FechaAsiento = DateTime.Now,
                Monto = ordenCompra.Detalles.Sum(d => d.CostoTotal),
                Estado = true,
                IdOrdenCompra = ordenCompra.Id
            };

            // Crédito en la cuenta 4
            var asientoCredito = new AsientoContable
            {
                sistemaAuxiliarId = 7, // Puedes usar un ID diferente para el crédito
                Descripcion = $"Crédito de Compra registrada - Orden {ordenCompra.Id}",
                IdTipoInventario = 1, // Ajusta según tu lógica
                CuentaContable = "4",
                TipoMovimiento = "CR",
                FechaAsiento = DateTime.Now,
                Monto = ordenCompra.Detalles.Sum(d => d.CostoTotal),
                Estado = true,
                IdOrdenCompra = ordenCompra.Id
            };

            _context.Tbl_AsientosContables.Add(asientoDebito);
            _context.Tbl_AsientosContables.Add(asientoCredito);

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Asiento contable creado exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear el asiento contable.");
            }
        }
    }
}