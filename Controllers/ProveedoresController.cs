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
    public class ProveedoresController : ControllerBase
    {
        private readonly ConexionDB _context;

        public ProveedoresController(ConexionDB context)
        {
            _context = context;
        }

        // GET: api/Proveedores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedores>>> GetTbl_Proveedores()
        {
            return await _context.Tbl_Proveedores.ToListAsync();
        }

        // GET: api/Proveedores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Proveedores>> GetProveedores(int id)
        {
            var proveedores = await _context.Tbl_Proveedores.FindAsync(id);

            if (proveedores == null)
            {
                return NotFound();
            }

            return proveedores;
        }

        // PUT: api/Proveedores/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProveedores(int id, Proveedores proveedores)
        {
            if (id != proveedores.Id)
            {
                return BadRequest();
            }

            var errores = ValidarProveedor(proveedores);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }

            _context.Entry(proveedores).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProveedoresExists(id))
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

        // POST: api/Proveedores
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Proveedores>> PostProveedores(Proveedores proveedores)
        {
            if (proveedores == null)
            {
                return BadRequest(new { mensaje = "Los datos enviados son nulos." });
            }
            var errores = ValidarProveedor(proveedores);
            if (errores.Any())
            {
                return BadRequest(new { errores });
            }
            _context.Tbl_Proveedores.Add(proveedores);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProveedores", new { id = proveedores.Id }, proveedores);
        }

        // DELETE: api/Proveedores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedores(int id)
        {
            var proveedores = await _context.Tbl_Proveedores.FindAsync(id);
            if (proveedores == null)
            {
                return NotFound();
            }

            _context.Tbl_Proveedores.Remove(proveedores);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProveedoresExists(int id)
        {
            return _context.Tbl_Proveedores.Any(e => e.Id == id);
        }

        private List<string> ValidarProveedor (Proveedores proveedores)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(proveedores.NombreComercial))
                errores.Add("El nombre comercial es obligatorio.");

            if (string.IsNullOrWhiteSpace(proveedores.Cedula_Rnc) ||
                (!ValidarCedula(proveedores.Cedula_Rnc) && !ValidarRNC(proveedores.Cedula_Rnc)))
            {
                errores.Add("El número ingresado no es una cédula ni un RNC válido.");
            }

            if (proveedores.Estado == null)
            {
                errores.Add("El estado es obligatorio.");
            }

            return errores;
        }

        public static bool ValidarCedula(string cedula)
        {
            // Remover cualquier guión
            cedula = cedula.Replace("-", "");

            // Verificar que tenga 11 dígitos
            if (cedula.Length != 11 || !cedula.All(char.IsDigit))
                return false;

            int suma = 0;
            int[] pesos = { 1, 2 }; // Alterna entre 1 y 2
            for (int i = 0; i < 10; i++)
            {
                int num = (cedula[i] - '0') * pesos[i % 2];
                suma += num > 9 ? num - 9 : num;
            }

            int digitoVerificador = (10 - (suma % 10)) % 10;
            return digitoVerificador == (cedula[10] - '0');
        }
        public static bool ValidarRNC(string rnc)
        {
            // Remover guiones y espacios
            rnc = rnc.Replace("-", "").Trim();

            // Verificar si tiene 9 o 11 dígitos numéricos
            if (!(rnc.Length == 9 || rnc.Length == 11) || !rnc.All(char.IsDigit))
                return false;

            int[] multiplicadores = { 7, 9, 8, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < 8; i++)
            {
                suma += (rnc[i] - '0') * multiplicadores[i];
            }

            int digitoVerificador = (suma % 11) == 0 ? 0 : (11 - (suma % 11)) % 10;
            return digitoVerificador == (rnc[8] - '0');
        }

    }
}
