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
        public async Task<ActionResult<IEnumerable<OrdenCompra>>> GetTbl_OrdenCompra()
        {
            return await _context.Tbl_OrdenCompra.ToListAsync();
        }

        // GET: api/OrdenCompras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenCompra>> GetOrdenCompra(int id)
        {
            var ordenCompra = await _context.Tbl_OrdenCompra.FindAsync(id);

            if (ordenCompra == null)
            {
                return NotFound();
            }

            return ordenCompra;
        }

        // PUT: api/OrdenCompras/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrdenCompra(int id, OrdenCompra ordenCompra)
        {
            if (id != ordenCompra.Id)
            {
                return BadRequest();
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
            if (ordenCompra == null)
            {
                return BadRequest(new { mensaje = "Los datos enviados son nulos." });
            }

            var errores = ValidarOrdenCompra(ordenCompra);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }
            _context.Tbl_OrdenCompra.Add(ordenCompra);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrdenCompra", new { id = ordenCompra.Id }, ordenCompra);
        }

        // DELETE: api/OrdenCompras/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrdenCompra(int id)
        {
            var ordenCompra = await _context.Tbl_OrdenCompra.FindAsync(id);
            if (ordenCompra == null)
            {
                return NotFound();
            }

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

            if (ordenCompra.IdProveedor <= 0)
                errores.Add("Debe seleccionar un proveedor válido.");

            if (ordenCompra.Estado == null)
            {
                errores.Add("El estado es obligatorio.");
            }

            return errores;
        }

    }
}
