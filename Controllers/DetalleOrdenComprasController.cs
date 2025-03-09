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

        // GET: api/DetalleOrdenCompras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleOrdenCompra>>> GetTbl_Detalle_OrdenCompra()
        {
            return await _context.Tbl_Detalle_OrdenCompra.ToListAsync();
        }

        // GET: api/DetalleOrdenCompras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleOrdenCompra>> GetDetalleOrdenCompra(int id)
        {
            var detalleOrdenCompra = await _context.Tbl_Detalle_OrdenCompra.FindAsync(id);

            if (detalleOrdenCompra == null)
            {
                return NotFound();
            }

            return detalleOrdenCompra;
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
        [HttpPost]
        public async Task<ActionResult<DetalleOrdenCompra>> PostDetalleOrdenCompra(DetalleOrdenCompra detalleOrdenCompra)
        {
            if (detalleOrdenCompra == null)
            {
                return BadRequest(new { mensaje = "Los datos enviados son nulos." });
            }
            var errores = ValidarDetalleOrdenCompra(detalleOrdenCompra);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }
            _context.Tbl_Detalle_OrdenCompra.Add(detalleOrdenCompra);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDetalleOrdenCompra", new { id = detalleOrdenCompra.Id }, detalleOrdenCompra);
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
