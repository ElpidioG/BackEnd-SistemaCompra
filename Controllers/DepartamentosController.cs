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
    public class DepartamentosController : ControllerBase
    {
        private readonly ConexionDB _context;

        public DepartamentosController(ConexionDB context)
        {
            _context = context;
        }

        // GET: api/Departamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Departamentos>>> GetTbl_Departamentos()
        {
            return await _context.Tbl_Departamentos.ToListAsync();
        }

        // GET: api/Departamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Departamentos>> GetDepartamentos(int id)
        {
            var departamentos = await _context.Tbl_Departamentos.FindAsync(id);

            if (departamentos == null)
            {
                return NotFound();
            }

            return departamentos;
        }

        // PUT: api/Departamentos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartamentos(int id, Departamentos departamentos)
        {
            if (id != departamentos.Id)
            {
                return BadRequest();
            }
            var errores = ValidarDepartamento(departamentos);

            if (errores.Any())
            {
                return BadRequest(new { errores });
            }
            _context.Entry(departamentos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartamentosExists(id))
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

        // POST: api/Departamentos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Departamentos>> PostDepartamentos(Departamentos departamentos)
        {

            if (departamentos == null)
            {
                return BadRequest(new { mensaje = "Los datos enviados son nulos." });
            }
            var errores = ValidarDepartamento(departamentos);

            if (errores.Any())
            {
                return BadRequest(new { errores });
            }
            _context.Tbl_Departamentos.Add(departamentos);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartamentos", new { id = departamentos.Id }, departamentos);
        }

        // DELETE: api/Departamentos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartamentos(int id)
        {
            var departamentos = await _context.Tbl_Departamentos.FindAsync(id);
            if (departamentos == null)
            {
                return NotFound();
            }

            _context.Tbl_Departamentos.Remove(departamentos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepartamentosExists(int id)
        {
            return _context.Tbl_Departamentos.Any(e => e.Id == id);
        }
        private List<string> ValidarDepartamento(Departamentos departamentos)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(departamentos.Descripcion))
                errores.Add("La descripción es obligatoria.");

            if (departamentos.Estado == null)
            {
                errores.Add("El estado es obligatorio.");
            }

            return errores;
        }
    }
}
