using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class ProxyApiController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ProxyApiController(IConfiguration config)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
            _apiBaseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:50544";
        }

        [Route("api/{*path}")]
        public async Task Proxy(string path)
        {
            var url = $"{_apiBaseUrl.TrimEnd('/')}/api/{path}";
            if (!string.IsNullOrEmpty(Request.QueryString.Value))
            {
                url += Request.QueryString.Value;
            }

            var requestMessage = new HttpRequestMessage(new HttpMethod(Request.Method), url);

            // Copy request headers
            foreach (var header in Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // Copy request body
            if (Request.ContentLength > 0 || Request.Headers.ContainsKey("Transfer-Encoding"))
            {
                var memoryStream = new MemoryStream();
                await Request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                requestMessage.Content = new StreamContent(memoryStream);
                if (Request.ContentType != null)
                {
                    requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Request.ContentType);
                }
            }

            try
            {
                var responseMessage = await _httpClient.SendAsync(requestMessage);

                // Set response status
                Response.StatusCode = (int)responseMessage.StatusCode;

                // Copy response headers
                foreach (var header in responseMessage.Headers)
                {
                    Response.Headers[header.Key] = header.Value.ToArray();
                }
                foreach (var header in responseMessage.Content.Headers)
                {
                    Response.Headers[header.Key] = header.Value.ToArray();
                }

                // Copy response body
                await responseMessage.Content.CopyToAsync(Response.Body);
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 502; // Bad Gateway
                using (var writer = new StreamWriter(Response.Body))
                {
                    await writer.WriteAsync($"Error proxying request to API: {ex.Message}");
                }
            }
        }
    }
}
