using System;

using System.Collections.Generic;

using System.Linq;

using System.Net.Http;

using System.Text;

using System.Text.Json;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using BackEnd_SistemaCompra.Contexts;

using BackEnd_SistemaCompra.Models;

using System.Net.Http.Headers;



namespace BackEnd_SistemaCompra.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class AsientosContablesController : ControllerBase

    {

        private readonly ConexionDB _context;

        private readonly HttpClient _httpClient;



        public AsientosContablesController(ConexionDB context, HttpClient httpClient)

        {

            _context = context;

            _httpClient = httpClient;

            _httpClient.BaseAddress = new System.Uri("https://iso810-contabilidad.azurewebsites.net"); // Configura la dirección base

        }



        // GET: api/AsientosContables

        [HttpGet]

        public async Task<ActionResult<IEnumerable<AsientoContable>>> GetAsientosContables()

        {

            return await _context.Tbl_AsientosContables.ToListAsync();

        }



        // GET: api/AsientosContables/5

        [HttpGet("{id}")]

        public async Task<ActionResult<AsientoContable>> GetAsientosContables(int id)

        {

            var asientosContables = await _context.Tbl_AsientosContables.FindAsync(id);



            if (asientosContables == null)

            {

                return NotFound();

            }



            return asientosContables;

        }



        // PUT: api/AsientosContables/5

        [HttpPut("{id}")]

        public async Task<IActionResult> PutAsientosContables(int id, AsientoContable asientosContables)

        {

            if (id != asientosContables.Id)

            {

                return BadRequest("El ID del asiento no coincide con el ID de la solicitud.");

            }



            var errores = ValidarAsientoContable(asientosContables);



            if (errores.Any())

            {

                return BadRequest(new { errores });

            }



            try

            {

                // Verificar si el asiento ya fue enviado a contabilidad



                if (asientosContables.Estado == false)

                {

                    await EnviarAsientoAContabilidad(new List<AsientoContable> { asientosContables }); // Enviar el asiento a contabilidad

                    asientosContables.Estado = true; // Actualizar el estado a "enviado"

                }



                _context.Entry(asientosContables).State = EntityState.Modified;

                await _context.SaveChangesAsync();



                return NoContent();

            }

            catch (DbUpdateConcurrencyException)

            {

                if (!AsientosContablesExists(id))

                {

                    return NotFound();

                }

                else

                {

                    throw;

                }

            }

            catch (Exception ex)

            {

                // Registra los detalles de la excepción para depurar

                Console.WriteLine($"Error al actualizar o enviar el asiento contable: {ex}");

                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al actualizar o enviar el asiento contable: {ex.Message}");

            }

        }

        // DELETE: api/AsientosContables/5

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteAsientosContables(int id)

        {

            var asientosContables = await _context.Tbl_AsientosContables.FindAsync(id);

            if (asientosContables == null)

            {

                return NotFound();

            }



            _context.Tbl_AsientosContables.Remove(asientosContables);

            await _context.SaveChangesAsync();



            return NoContent();

        }



        private bool AsientosContablesExists(int id)

        {

            return _context.Tbl_AsientosContables.Any(e => e.Id == id);

        }



        private List<string> ValidarAsientoContable(AsientoContable asientosContables)

        {

            var errores = new List<string>();



            if (string.IsNullOrWhiteSpace(asientosContables.Descripcion))

                errores.Add("La descripción es obligatoria.");



            if (string.IsNullOrWhiteSpace(asientosContables.CuentaContable))

                errores.Add("La cuenta contable es obligatoria.");



            if (string.IsNullOrWhiteSpace(asientosContables.TipoMovimiento))

                errores.Add("El tipo de movimiento es obligatorio.");



            if (asientosContables.Monto <= 0)

                errores.Add("El monto debe ser mayor que cero.");



            return errores;

        }



        [HttpPost("EnviarAsientos")]

        public async Task<IActionResult> EnviarAsientos([FromBody] List<AsientoContable> asientos)

        {

            try

            {

                await EnviarAsientoAContabilidad(asientos);

                return Ok("Asientos enviados correctamente.");

            }

            catch (Exception ex)

            {

                return BadRequest($"Error al enviar los asientoxs: {ex.Message}");

            }

        }

        private async Task EnviarAsientoAContabilidad(List<AsientoContable> asientosContables)
        {
            if (asientosContables == null || asientosContables.Count == 0)
            {
                return; // No hay asientos para procesar
            }

            // Agrupar asientos por descripción
            var asientosAgrupados = asientosContables.GroupBy(a => a.Descripcion);

            var asientosContablesDTOs = new List<object>(); // Lista para almacenar los DTOs de los asientos

            foreach (var grupo in asientosAgrupados)
            {
                var primerAsiento = grupo.First(); // Usamos el primer asiento para la información general
                var detalles = new List<object>();
                var debito = grupo.FirstOrDefault(a => a.TipoMovimiento == "DB");
                var credito = grupo.FirstOrDefault(a => a.TipoMovimiento == "CR");

                if (debito != null)
                {
                    detalles.Add(new
                    {
                        cuentaId = int.Parse(debito.CuentaContable),
                        montoAsiento = Convert.ToDouble(debito.Monto),
                        tipoMovimiento = "DB"
                    });
                }

                if (credito != null)
                {
                    detalles.Add(new
                    {
                        cuentaId = int.Parse(credito.CuentaContable),
                        montoAsiento = Convert.ToDouble(credito.Monto),
                        tipoMovimiento = "CR"
                    });
                }

                // Validación adicional: Asegurar que haya al menos dos detalles
                if (detalles.Count < 2)
                {
                    Console.WriteLine($"Advertencia: No se encontraron débito y crédito en el grupo {grupo.Key}.");
                    continue; // Omitir el envío de este asiento
                }

                var asientoContableDTO = new
                {
                    descripcion = primerAsiento.Descripcion,
                    sistemaAuxiliarId = primerAsiento.sistemaAuxiliarId,
                    fechaAsiento = primerAsiento.FechaAsiento,
                    detalles = detalles
                };

                asientosContablesDTOs.Add(asientoContableDTO); // Agregar el DTO a la lista
            }

            // Enviar todos los DTOs a la API de contabilidad, envueltos en el objeto 'dto'
            if (asientosContablesDTOs.Count > 0)
            {
                // Agregar registro para ver los datos que se están enviando
                Console.WriteLine("Datos enviados a la API de contabilidad:");
                var dtoWrapper = new { dto = asientosContablesDTOs }; // Envolver la lista en un objeto 'dto'
                string json = JsonSerializer.Serialize(dtoWrapper, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json); // Imprimir el JSON completo

             
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                    if (string.IsNullOrEmpty(token))
                    {
                        throw new Exception("Token de autenticación no válido o vacío.");
                    }
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    throw new Exception("Encabezado de autorización no encontrado.");
                }

                var response = await _httpClient.PostAsync("/api/EntradaContable", content);

                if (response.IsSuccessStatusCode)
                {
                    // Agregar mensaje para confirmar que la API de contabilidad recibió los asientos
                    Console.WriteLine($"Asientos recibidos por la API de contabilidad.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(); // Leer el contenido de error
                    throw new Exception($"Error al enviar el asiento a contabilidad. Código de estado: {response.StatusCode}, Contenido: {errorContent}, enviado: {json}");
                }



            }
        }
        [HttpGet("EntradaContable")]
        public async Task<IActionResult> GetEntradaContable()
        {
            try
            {
                // Obtener el token de autenticación del encabezado de la solicitud
                if (HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                    if (string.IsNullOrEmpty(token))
                    {
                        return BadRequest("Token de autenticación no válido o vacío.");
                    }

                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    return BadRequest("Encabezado de autorización no encontrado.");
                }

                var response = await _httpClient.GetAsync("/api/EntradaContable");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var resultados = JsonSerializer.Deserialize<List<object>>(content, options); // Usar object para la deserialización

                    // Imprimir los resultados en la consola
                    Console.WriteLine("Resultados de la API de EntradaContable:");

                    Console.WriteLine(JsonSerializer.Serialize(resultados, new JsonSerializerOptions { WriteIndented = true }));

                    return Ok(resultados);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return BadRequest($"Error al obtener los datos de la API de contabilidad. Código de estado: {response.StatusCode}, Contenido: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener los datos de la API de contabilidad: {ex.Message}");
            }
        }
    }
}