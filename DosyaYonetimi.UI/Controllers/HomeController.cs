using DosyaYonetimi.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DosyaYonetimi.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
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
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("DosyaAPI");
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("User/SignIn", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultViewModel>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Status)
                {
                    var gercekToken = result.Message;

                    if (string.IsNullOrEmpty(gercekToken))
                    {
                        ModelState.AddModelError("", "API başarı döndü ama Token boş geldi!");
                        return View(model);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.UserName),
                        new Claim("Token", gercekToken)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", result?.Message ?? "Giriş başarısız!");
            }
            else
            {
                ModelState.AddModelError("", "API bağlantı hatası veya kullanıcı bulunamadı.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("DosyaAPI");

            
            var apiBekleyenModel = new
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                FullName = model.UserName, 
                PhoneNumber = "0000000000" 
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(apiBekleyenModel), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("User/Add", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                
                if (result != null && result.Status)
                {
                    TempData["Success"] = "Hesabınız başarıyla oluşturuldu! Şimdi giriş yapabilirsiniz.";
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    
                    ModelState.AddModelError("", result?.Message ?? "Şifreniz kurallara uymuyor (Büyük harf, rakam ve özel karakter içermelidir).");
                    return View(model);
                }
            }

            ModelState.AddModelError("", "API sunucusuna ulaşılamadı veya eksik veri gönderildi.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var client = GetAuthenticatedClient();

            
            if (User.Identity.Name != null && User.Identity.Name.ToLower() == "admin")
            {
                var responseAdmin = await client.GetAsync("Home/AdminSummary");
                if (responseAdmin.IsSuccessStatusCode)
                {
                    var content = await responseAdmin.Content.ReadAsStringAsync();
                    var adminSummary = JsonSerializer.Deserialize<AdminSummaryViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View("AdminIndex", adminSummary);
                }
                return View("AdminIndex", new AdminSummaryViewModel());
            }

            
            var responseUser = await client.GetAsync("Home/UserSummary");
            if (responseUser.IsSuccessStatusCode)
            {
                var content = await responseUser.Content.ReadAsStringAsync();
                var userSummary = JsonSerializer.Deserialize<UserSummaryViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(userSummary);
            }

            return View(new UserSummaryViewModel());
        }

        [Authorize]
        public async Task<IActionResult> Files(int? folderId = null)
        {
            var client = GetAuthenticatedClient();
            var model = new DriveViewModel();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var folderRes = await client.GetAsync("Folder");
            var allFolders = new List<FolderViewModel>();
            if (folderRes.IsSuccessStatusCode)
                allFolders = JsonSerializer.Deserialize<List<FolderViewModel>>(await folderRes.Content.ReadAsStringAsync(), options);

            var fileRes = await client.GetAsync("File");
            var allFiles = new List<FileItemViewModel>();
            if (fileRes.IsSuccessStatusCode)
                allFiles = JsonSerializer.Deserialize<List<FileItemViewModel>>(await fileRes.Content.ReadAsStringAsync(), options);

            var favRes = await client.GetAsync("Favorite");
            if (favRes.IsSuccessStatusCode && allFiles.Any())
            {
                var favorites = JsonSerializer.Deserialize<List<FavoriteViewModel>>(await favRes.Content.ReadAsStringAsync(), options);
                if (favorites != null)
                {
                    foreach (var file in allFiles) file.IsFavorite = favorites.Any(f => f.FileItemId == file.Id);
                }
            }

            model.Folders = allFolders.Where(f => f.ParentFolderId == folderId).ToList();
            model.Files = allFiles.Where(f => f.FolderId == folderId).ToList();

            ViewBag.CurrentFolderId = folderId;
            if (folderId.HasValue)
            {
                var currentFolder = allFolders.FirstOrDefault(f => f.Id == folderId);
                ViewBag.ParentFolderId = currentFolder?.ParentFolderId;
                ViewBag.CurrentFolderName = currentFolder?.Name;
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Trash()
        {
            var client = GetAuthenticatedClient();

            var model = new DriveViewModel();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var fileRes = await client.GetAsync("File/DeletedFiles");
            if (fileRes.IsSuccessStatusCode)
                model.Files = JsonSerializer.Deserialize<List<FileItemViewModel>>(await fileRes.Content.ReadAsStringAsync(), options);

            var folderRes = await client.GetAsync("Folder/DeletedFolders");
            if (folderRes.IsSuccessStatusCode)
                model.Folders = JsonSerializer.Deserialize<List<FolderViewModel>>(await folderRes.Content.ReadAsStringAsync(), options);

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> SharedFiles()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("FileShare");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var shared = JsonSerializer.Deserialize<List<FileShareViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(shared);
            }
            return View(new List<FileShareViewModel>());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFolder(string folderName, int? parentFolderId)
        {
            if (string.IsNullOrEmpty(folderName)) return RedirectToAction("Files", new { folderId = parentFolderId });

            var client = GetAuthenticatedClient();

            var tokenString = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenString);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var folderDto = new { Name = folderName, AppUserId = userId, ParentFolderId = parentFolderId };
            var jsonContent = new StringContent(JsonSerializer.Serialize(folderDto), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Folder", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ResultViewModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null && result.Status) TempData["Success"] = result.Message ?? "Klasör başarıyla oluşturuldu!";
                else TempData["Error"] = result?.Message ?? "Klasör oluşturulamadı!";
            }
            else TempData["Error"] = "Sunucu hatası: Klasör oluşturma isteği reddedildi.";

            return RedirectToAction("Files", new { folderId = parentFolderId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Upload(IFormFile file, int? folderId)
        {
            if (file == null || file.Length == 0) return RedirectToAction("Files", new { folderId = folderId });

            var client = GetAuthenticatedClient();

            string base64Data;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                base64Data = Convert.ToBase64String(ms.ToArray());
            }

            var uploadDto = new
            {
                FileName = Path.GetFileNameWithoutExtension(file.FileName),
                FileData = base64Data,
                FileExt = Path.GetExtension(file.FileName),
                FolderId = folderId
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(uploadDto, new JsonSerializerOptions { PropertyNamingPolicy = null }), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("File/Upload", jsonContent);

            if (response.IsSuccessStatusCode) TempData["Success"] = "Dosya başarıyla yüklendi!";
            else TempData["Error"] = "Dosya yüklenirken bir hata oluştu! Kota veya limit kontrolü yapın.";

            return RedirectToAction("Files", new { folderId = folderId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync($"File/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fileInfo = JsonSerializer.Deserialize<FileItemViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fileInfo != null && !string.IsNullOrEmpty(fileInfo.SystemFileName))
                {
                    var baseUrl = client.BaseAddress.ToString().Replace("api/", "");
                    if (!baseUrl.EndsWith("/")) baseUrl += "/";

                    var physicalFileUrl = $"{baseUrl}Files/UserUploads/{fileInfo.SystemFileName}";

                    try
                    {
                        var httpClient = new HttpClient();
                        var fileBytes = await httpClient.GetByteArrayAsync(physicalFileUrl);

                        string contentType = "application/octet-stream";
                        return File(fileBytes, contentType, fileInfo.Name);
                    }
                    catch
                    {
                        TempData["Error"] = "Dosya sunucuda fiziksel olarak bulunamadı.";
                        return RedirectToAction("Files");
                    }
                }
            }

            TempData["Error"] = "Dosya bilgileri alınamadı.";
            return RedirectToAction("Files");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteFile(int id, int? currentFolderId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"File/{id}");
            if (response.IsSuccessStatusCode) TempData["Success"] = "Dosya çöp kutusuna taşındı.";
            return RedirectToAction("Files", new { folderId = currentFolderId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteFolder(int id, int? currentFolderId)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"Folder/{id}");
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ResultViewModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null && result.Status) TempData["Success"] = result.Message ?? "Klasör silindi.";
                else TempData["Error"] = result?.Message ?? "Klasör silinemedi.";
            }
            return RedirectToAction("Files", new { folderId = currentFolderId });
        }

       

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RestoreFile(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"File/Restore/{id}", null);

            if (response.IsSuccessStatusCode) TempData["Success"] = "Dosya başarıyla geri yüklendi.";
            else TempData["Error"] = "Dosya geri yüklenirken hata oluştu.";

            return RedirectToAction("Trash");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RestoreFolder(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.PostAsync($"Folder/Restore/{id}", null);

            if (response.IsSuccessStatusCode) TempData["Success"] = "Klasör başarıyla geri yüklendi.";
            else TempData["Error"] = "Klasör geri yüklenirken hata oluştu.";

            return RedirectToAction("Trash");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> HardDeleteFile(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"File/HardDelete/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Status) TempData["Success"] = result.Message;
                else TempData["Error"] = result?.Message ?? "Kalıcı silme başarısız.";
            }
            else
            {
                TempData["Error"] = "Sunucu hatası oluştu veya admin yetkisi gerekiyor.";
            }

            return RedirectToAction("Trash");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> HardDeleteFolder(int id)
        {
            var client = GetAuthenticatedClient();
            var response = await client.DeleteAsync($"Folder/HardDelete/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Status) TempData["Success"] = result.Message ?? "Klasör kalıcı olarak silindi.";
                else TempData["Error"] = result?.Message ?? "Klasör kalıcı olarak silinemedi.";
            }
            else
            {
                TempData["Error"] = "Sunucu hatası oluştu veya admin yetkisi gerekiyor.";
            }

            return RedirectToAction("Trash");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int fileId)
        {
            var client = GetAuthenticatedClient();
            var dto = new { FileItemId = fileId, FolderId = (int?)null };
            var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            await client.PostAsync("Favorite/AddFavorite", jsonContent);

            return RedirectToAction("Files");
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetUserList()
        {
            var client = GetAuthenticatedClient();
            var response = await client.GetAsync("User/List"); 
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                
                var otherUsers = users.Where(u => u.UserName != User.Identity.Name).Select(u => new { u.Id, u.UserName, u.Email }).ToList();
                return Json(otherUsers);
            }
            return Json(new { error = "Kullanıcı listesi alınamadı" });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ShareFile(int fileId, string receiverId, int days)
        {
            if (string.IsNullOrEmpty(receiverId))
            {
                TempData["Error"] = "Kime göndereceğini seçmeyi unuttun!";
                return RedirectToAction("Files");
            }

            var client = GetAuthenticatedClient();

            
            var shareDto = new
            {
                FileItemId = fileId,
                SharedWithUserId = receiverId, 
                CanEdit = false,               
                ExpiryDays = days              
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            var jsonContent = new StringContent(JsonSerializer.Serialize(shareDto, jsonOptions), Encoding.UTF8, "application/json");

            
            var response = await client.PostAsync("FileShare/CreateShare", jsonContent);

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<ResultViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response.IsSuccessStatusCode && result != null && result.Status)
                {
                    TempData["Success"] = result.Message ?? "Dosya başarıyla paylaşıldı!";
                }
                else
                {
                    TempData["Error"] = result?.Message ?? $"API Hatası: {content}";
                }
            }
            catch
            {
                TempData["Error"] = $"Sistem Hatası: {response.StatusCode} - {content}";
            }

            return RedirectToAction("Files");
        }






    }
}