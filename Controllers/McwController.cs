using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Embedded_PaymentForm_.NET.Models;

namespace Embedded_PaymentForm_.NET.Controllers
{
    public class McwController : Controller
    {
        private readonly IConfiguration _configuration;

        public McwController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // @@ Manejo de solicitudes GET para la ruta raíz @@
        [HttpGet]
        public IActionResult Index()
        {
            string orderId = "Order-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            ViewBag.OrderId = orderId;
            return View();
        }

        // @@ Manejo de solicitudes POST para checkout @@
        [HttpPost("/checkout")]
        public async Task<IActionResult> Checkout([FromForm] PaymentRequest paymentRequest)
        {
            // Obteniendo claves API
            var apiCredentials = _configuration.GetSection("ApiCredentials");
            string username = apiCredentials["USERNAME"];
            string password = apiCredentials["PASSWORD"];
            string publicKey = apiCredentials["PUBLIC_KEY"];

            string url = "https://api.micuentaweb.pe/api-payment/V4/Charge/CreatePayment";
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            // Almacenar los valores para el formulario de pago
            var data = new
            {
                amount = Math.Round(paymentRequest.Amount * 100.0, 0),
                currency = paymentRequest.Currency,
                customer = new
                {
                    email = paymentRequest.Email,
                    billingDetails = new
                    {
                        firstName = paymentRequest.FirstName,
                        lastName = paymentRequest.LastName,
                        identityType = paymentRequest.IdentityType,
                        identityCode = paymentRequest.IdentityCode,
                        phoneNumber = paymentRequest.PhoneNumber,
                        address = paymentRequest.Address,
                        country = paymentRequest.Country,
                        state = paymentRequest.State,
                        city = paymentRequest.City,
                        zipCode = paymentRequest.ZipCode
                    }
                },
                orderId = paymentRequest.OrderId
            };


            // Crear la conexión a la API para la creación del FormToken
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var responseData = JsonSerializer.Deserialize<PaymentResponse>(responseContent, options);

                // Extrae el FormToken y PublicKey
                if (responseData.Status == "SUCCESS")
                {
                    ViewData["FormToken"] = responseData?.Answer?.FormToken;
                    ViewData["PublicKey"] = publicKey;
                    // Renderiza el Template
                    return View("Checkout");
                }
                else
                {
                    return View("Error");
                }
            }
        }

        // @@ Manejo de solicitudes POST para result @@
        [HttpPost("/result")]
        public IActionResult Result([FromForm] IFormCollection form)
        {
            string hmacKey = _configuration.GetSection("ApiCredentials")["HMACSHA256"];

            // Válida que la respuesta sea íntegra comprando el hash recibido en el 'kr-hash' con el generado con el 'kr-answer'
            if (!CheckHash(form, hmacKey))
            {
                Console.WriteLine("Invalid signature");
                return View("Error");
            }

            // Crear un objeto con toda la información del formulario
            var formData = new
            {
                KrHash = form["kr-hash"].ToString(),
                KrHashAlgorithm = form["kr-hash-algorithm"].ToString(),
                KrAnswerType = form["kr-answer-type"].ToString(),
                KrAnswer = JsonSerializer.Deserialize<JsonDocument>(form["kr-answer"]),
                KrHashKey = form["kr-hash-key"].ToString()
            };

            // Renderiza el Template
            return View("Result", formData);
        }

        [HttpPost("/ipn")]
        public IActionResult Ipn([FromForm] IFormCollection form)
        {
            string privateKey = _configuration.GetSection("ApiCredentials")["PASSWORD"];

            // Válida que la respuesta sea íntegra comprando el hash recibido en el 'kr-hash' con el generado con el 'kr-answer'
            if (!CheckHash(form, privateKey))
            {
                Console.WriteLine("Invalid signature");
                return View("Error");
            }

            string krAnswer = form["kr-answer"].ToString();

            using var jsonDocument = JsonDocument.Parse(krAnswer);
            var root = jsonDocument.RootElement;

            // Extrae datos de la transacción
            string orderStatus = root.GetProperty("orderStatus").GetString();
            string orderId = root.GetProperty("orderDetails").GetProperty("orderId").GetString();
            string uuid = root.GetProperty("transactions")[0].GetProperty("uuid").GetString();

            // Retorna el valor de OrderStatus
            return Ok($"OK! Order Status: {orderStatus}");
            
        }

        // Verifica la integridad del Hash recibido y el generado  	
        private bool CheckHash(IFormCollection form, string key)
        {
            // Verificar que los valores existen
            if (!form.ContainsKey("kr-answer") || !form.ContainsKey("kr-hash"))
            {
                Console.WriteLine("Missing required form fields");
                return false;
            }

            var answer = form["kr-answer"].ToString();
            var hash = form["kr-hash"].ToString();

            // Genera un hash HMAC-SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] answerBytes = Encoding.UTF8.GetBytes(answer);
                byte[] computedHashBytes = hmac.ComputeHash(answerBytes);
                string computedHashString = Convert.ToHexString(computedHashBytes).ToLowerInvariant();
                
                // Retorna la integridad
                return string.Equals(computedHashString, hash, StringComparison.OrdinalIgnoreCase);
            }
        }

    }
}
