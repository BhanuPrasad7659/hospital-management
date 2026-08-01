using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class BaseController : Controller
    {
        protected async Task<T?> GetAsync<T>(string path)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using (var client = new HttpClient(handler))
            {
                if (Request.Headers.TryGetValue("Cookie", out var cookie))
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookie.ToString());
                }
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var apiBaseUrl = config["ApiSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var url = $"{apiBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
                try
                {
                    return await client.GetFromJsonAsync<T>(url);
                }
                catch
                {
                    return default;
                }
            }
        }

        protected async Task<HttpResponseMessage?> SendAsync(string path, HttpMethod method, object? payload = null)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using (var client = new HttpClient(handler))
            {
                if (Request.Headers.TryGetValue("Cookie", out var cookie))
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookie.ToString());
                }
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var apiBaseUrl = config["ApiSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var url = $"{apiBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
                try
                {
                    if (method == HttpMethod.Post)
                    {
                        return await client.PostAsJsonAsync(url, payload);
                    }
                    if (method == HttpMethod.Put)
                    {
                        return payload != null ? await client.PutAsJsonAsync(url, payload) : await client.PutAsync(url, null);
                    }
                    if (method == HttpMethod.Delete)
                    {
                        return await client.DeleteAsync(url);
                    }
                }
                catch {}
                return null;
            }
        }
    }
}
