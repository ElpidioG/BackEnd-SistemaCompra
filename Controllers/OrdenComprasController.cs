
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
                .Include(o => o.Proveedor) // Incluye la relación del proveedor
                .Select(o => new
                {
                    o.Id,
                    o.Fecha,
                    ProveedorNombre = o.Proveedor != null ? o.Proveedor.NombreComercial : "Sin proveedor", // Asegúrate de acceder a la propiedad correcta
                    o.Estado
                })
                .ToListAsync();

            return Ok(ordenes);
        }




        // GET: api/OrdenCompras
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]

        public async Task<ActionResult<OrdenCompra>> PostOrdenCompra(OrdenCompra ordenCompra)
        {
            if (ordenCompra.idProveedor == 0)
            {
                return BadRequest("El proveedor es obligatorio.");
            }

            // Guarda la orden de compra
            _context.Tbl_OrdenCompra.Add(ordenCompra);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostOrdenCompra), new { id = ordenCompra.Id }, ordenCompra);
        }
        // DELETE: api/OrdenCompras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrdenCompra(int id)
        {
            var ordenCompra = await _context.Tbl_OrdenCompra
                .Include(o => o.Detalles) // Asegúrate de incluir los detalles relacionados
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordenCompra == null)
            {
                return NotFound();
            }

            // Eliminar los detalles primero
            _context.Tbl_Detalle_OrdenCompra.RemoveRange(ordenCompra.Detalles);

            // Ahora eliminar la orden de compra
            _context.Tbl_OrdenCompra.Remove(ordenCompra);
            await _context.SaveChangesAsync();

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

    }
}