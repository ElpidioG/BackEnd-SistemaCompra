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
    public class UnidadesMedidasController : ControllerBase
    {
        private readonly ConexionDB _context;

        public UnidadesMedidasController(ConexionDB context)
        {
            _context = context;
        }

        // GET: api/UnidadesMedidas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnidadesMedidas>>> GetTbl_UnidadesMedidas()
        {
            return await _context.Tbl_UnidadesMedidas.ToListAsync();
        }

        // GET: api/UnidadesMedidas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UnidadesMedidas>> GetUnidadesMedidas(int id)
        {
            var unidadesMedidas = await _context.Tbl_UnidadesMedidas.FindAsync(id);

            if (unidadesMedidas == null)
            {
                return NotFound();
            }

            return unidadesMedidas;
        }

        // PUT: api/UnidadesMedidas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUnidadesMedidas(int id, UnidadesMedidas unidadesMedidas)
        {
            if (id != unidadesMedidas.Id)
            {
                return BadRequest();
            }
            var errores = ValidarUnidadesMedida(unidadesMedidas);

            if (errores.Any())
            {
                return BadRequest(new { errores });
            }
            _context.Entry(unidadesMedidas).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnidadesMedidasExists(id))
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

        // POST: api/UnidadesMedidas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UnidadesMedidas>> PostUnidadesMedidas(UnidadesMedidas unidadesMedidas)
        {
            if(unidadesMedidas == null)
            {
                return BadRequest(new { mensaje = "Los datos enviados son nulos." });
            }

            var errores = ValidarUnidadesMedida(unidadesMedidas);

            if (errores.Any())
            {
                return BadRequest(new { errores });
            }

            _context.Tbl_UnidadesMedidas.Add(unidadesMedidas);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnidadesMedidas", new { id = unidadesMedidas.Id }, unidadesMedidas);
        }

        // DELETE: api/UnidadesMedidas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnidadesMedidas(int id)
        {
            var unidadesMedidas = await _context.Tbl_UnidadesMedidas.FindAsync(id);
            if (unidadesMedidas == null)
            {
                return NotFound();
            }

            _context.Tbl_UnidadesMedidas.Remove(unidadesMedidas);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UnidadesMedidasExists(int id)
        {
            return _context.Tbl_UnidadesMedidas.Any(e => e.Id == id);
        }
        private List<string> ValidarUnidadesMedida(UnidadesMedidas unidadesMedidas)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(unidadesMedidas.Descripcion))
                errores.Add("La descripción es obligatoria.");

            if (unidadesMedidas.Estado == null)
            {
                errores.Add("El estado es obligatorio.");
            }

            return errores;
        }
    }
}
