using DosyaYonetimi.UI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DosyaYonetimi.UI.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        
        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("DosyaAPI");
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("User/List");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(users);
            }

            
            TempData["Error"] = "Bu sayfayı görüntülemek için Admin yetkisine sahip olmalısınız.";
            return RedirectToAction("Index", "Home");
        }

        
        [HttpPost]
        public async Task<IActionResult> UpdateQuota(string userId, long newStorageQuota)
        {
            if (string.IsNullOrEmpty(userId) || newStorageQuota <= 0)
            {
                TempData["Error"] = "Geçersiz kota veya kullanıcı bilgisi.";
                return RedirectToAction("Users");
            }

            var client = GetAuthenticatedClient();

            
            var dto = new { UserId = userId, NewStorageQuota = newStorageQuota };
            var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            var response = await client.PutAsync("User/UpdateQuota", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Status)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result?.Message ?? "Kota güncellenemedi.";
            }
            else
            {
                TempData["Error"] = "Sunucu hatası veya yetkisiz işlem.";
            }

            return RedirectToAction("Users");
        }
    }
}