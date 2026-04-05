using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DosyaYonetimi.API.Models; 
using DosyaYonetimi.API.Repositories; 

namespace DosyaYonetimi.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize] 
    public class HomeController : ControllerBase
    {
        private readonly FileItemRepository _fileItemRepository;
        private readonly FolderRepository _folderRepository;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(FileItemRepository fileItemRepository, FolderRepository folderRepository, UserManager<AppUser> userManager)
        {
            _fileItemRepository = fileItemRepository;
            _folderRepository = folderRepository;
            _userManager = userManager;
        }

        
        [HttpGet]
        public async Task<IActionResult> UserSummary()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var totalFolders = await _folderRepository.Where(f => f.AppUserId == userId && f.IsActive).CountAsync();
            var totalFiles = await _fileItemRepository.Where(f => f.AppUserId == userId && f.IsActive).CountAsync();

            
            return Ok(new
            {
                FullName = user.FullName,
                TotalFolders = totalFolders,
                TotalFiles = totalFiles,
                UsedStorageMB = user.UsedStorage,
                RemainingStorageMB = Math.Max(0, user.StorageQuota - user.UsedStorage),
                TotalQuotaMB = user.StorageQuota
            });
        }

        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminSummary()
        {
            var totalSystemUsers = await _userManager.Users.CountAsync();
            var totalSystemFiles = await _fileItemRepository.Where(f => f.IsActive).CountAsync();
            var totalSystemFolders = await _folderRepository.Where(f => f.IsActive).CountAsync();
            var totalTrashFiles = await _fileItemRepository.Where(f => !f.IsActive).CountAsync();

            return Ok(new
            {
                TotalUsers = totalSystemUsers,
                TotalActiveFiles = totalSystemFiles,
                TotalActiveFolders = totalSystemFolders,
                TotalFilesInTrash = totalTrashFiles
            });
        }

        //TEST
        
        [HttpGet]
        [AllowAnonymous]
        public string Ping()
        {
            return "Dosya Yönetim Portalı API Sorunsuz Çalışıyor!";
        }
    }
}