using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd_SistemaCompra.Contexts;
using BackEnd_SistemaCompra.Models;

namespace BackEnd_SistemaCompra.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticulosController : ControllerBase
    {
        private readonly ConexionDB _context;

        public ArticulosController(ConexionDB context)
        {
            _context = context;
        }

        // GET: api/Articulos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Articulos>>> GetTbl_Articulos()
        {
            return await _context.Tbl_Articulos.ToListAsync();
        }

        // GET: api/Articulos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Articulos>> GetArticulos(int id)
        {
            var articulos = await _context.Tbl_Articulos.FindAsync(id);
            if (articulos == null)
            {
                return NotFound();
            }

            return articulos;
        }

        // PUT: api/Articulos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticulos(int id, Articulos articulos)
        {
            if (id != articulos.Id)
            {
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del objeto." });
            }

            var errores = ValidarArticulo(articulos);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }

            _context.Entry(articulos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticulosExists(id))
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

        // POST: api/Articulos
        [HttpPost]
        public async Task<ActionResult<Articulos>> PostArticulos(Articulos articulos)
        {
            if (articulos == null)
            {
                return BadRequest(new { mensaje = "Los datos enviados son nulos." });
            }

            var errores = ValidarArticulo(articulos);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }

            _context.Tbl_Articulos.Add(articulos);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArticulos), new { id = articulos.Id }, articulos);
        }

        // DELETE: api/Articulos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticulos(int id)
        {
            var articulos = await _context.Tbl_Articulos.FindAsync(id);
            if (articulos == null)
            {
                return NotFound();
            }

            _context.Tbl_Articulos.Remove(articulos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArticulosExists(int id)
        {
            return _context.Tbl_Articulos.Any(e => e.Id == id);
        }
        private List<string> ValidarArticulo(Articulos articulo)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(articulo.Descripcion))
                errores.Add("La descripción es obligatoria.");

            if (string.IsNullOrWhiteSpace(articulo.Marca))
                errores.Add("La marca es obligatoria.");

            if (articulo.Existencia <= 0)
                errores.Add("La existencia debe ser mayor a 0.");

            if (articulo.IdUnidadMedidas <= 0)
                errores.Add("Debe seleccionar una unidad de medida válida.");

            if (articulo.Estado == null)
            {
                errores.Add("El estado es obligatorio.");
            }

            return errores;
        }
    }
}
