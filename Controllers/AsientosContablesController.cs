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
            var asientosAgrupados = asientosContables.GroupBy(a => a.IdOrdenCompra);
            Console.WriteLine("Este es el grupo: ", asientosContables.GroupBy(a => a.IdOrdenCompra));


            foreach (var grupo in asientosAgrupados)
            {
                Console.WriteLine("Este es el grupo:", grupo.ToString());

                var descripcionConcatenada = string.Join(" y ", grupo.Select(a => a.Descripcion).Distinct());
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
                    Console.WriteLine($"After adding debito, count: {detalles.Count}");
                }

                if (credito != null)
                {
                    detalles.Add(new
                    {
                        cuentaId = int.Parse(credito.CuentaContable),
                        montoAsiento = Convert.ToDouble(credito.Monto),
                        tipoMovimiento = "CR"
                    });
                    Console.WriteLine($"After adding credito, count: {detalles.Count}");
                }

                Console.WriteLine("Estos son los detalles: ", detalles);
                Console.WriteLine("Esta transacción tiene este numero de detalles: ", detalles.Count);
                // Validación adicional: Asegurar que haya al menos dos detalles
                if (detalles.Count < 2)
                {
                    Console.WriteLine($"Advertencia: No se encontraron débito y crédito en el grupo {grupo.Key}.");
                    continue; // Omitir el envío de este asiento
                }

                var asientoParaEnviar = new
                {
                    descripcion = descripcionConcatenada,
                    sistemaAuxiliarId = primerAsiento.sistemaAuxiliarId,
                    fechaAsiento = primerAsiento.FechaAsiento,
                    detalles = detalles
                };

                // Enviar cada asiento individualmente a la API de contabilidad
                Console.WriteLine("Datos enviados a la API de contabilidad:");
                string json = JsonSerializer.Serialize(asientoParaEnviar, new JsonSerializerOptions { WriteIndented = true });
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
                    // Agregar mensaje para confirmar que la API de contabilidad recibió el asiento
                    Console.WriteLine($"Asiento con descripción '{descripcionConcatenada}' recibido por la API de contabilidad.");
                    var result = await response.Content.ReadAsStringAsync(); // Leer el contenido de recepcion
                    Console.WriteLine($"Esto fue la respuesta: {result} ");

                    // Actualizar el estado de los asientos locales a "Enviado" (false)
                    foreach (var asiento in grupo)
                    {
                        asiento.Estado = false; // Cambiamos a false para indicar "enviado"
                        _context.Entry(asiento).State = EntityState.Modified;
                    }
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Estado de los asientos para la orden {grupo.Key} actualizado a 'Enviado' (false).");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(); // Leer el contenido de error
                    throw new Exception($"Error al enviar el asiento con descripción '{descripcionConcatenada}' a contabilidad. Código de estado: {response.StatusCode}, Contenido: {errorContent}, enviado: {json}");
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

                var response = await _httpClient.GetAsync("/api/EntradaContable?sort=-FechaAsiento&limit=10");

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

        [HttpGet("CuentaContable")]
        public async Task<IActionResult> GetCuentaContable()
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

                var response = await _httpClient.GetAsync("/api/CuentaContable");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var resultados = JsonSerializer.Deserialize<List<object>>(content, options); // Usar object para la deserialización

                    // Imprimir los resultados en la consola
                    Console.WriteLine("Resultados de la API de CuentaContable:");

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

        // GET: api/AsientosContables/orden/{idOrdenCompra}
        [HttpGet("orden/{idOrdenCompra}")]
        public async Task<ActionResult<IEnumerable<AsientoContable>>> GetAsientosPorOrdenCompra(int idOrdenCompra)
        {
            var asientosContables = await _context.Tbl_AsientosContables
                .Where(a => a.IdOrdenCompra == idOrdenCompra)
                .ToListAsync();

            return asientosContables;
        }
    }
}