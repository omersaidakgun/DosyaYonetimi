using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DosyaYonetimi.API.DTOs; 
using DosyaYonetimi.API.Models; 
using DosyaYonetimi.API.Repositories; 

namespace DosyaYonetimi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly FileItemRepository _fileItemRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        ResultDto _result = new ResultDto();

        public FileController(FileItemRepository fileItemRepository, UserManager<AppUser> userManager, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _fileItemRepository = fileItemRepository;
            _userManager = userManager;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        
        [HttpGet]
        public async Task<List<FileItemDto>> List()
        {
            var files = await _fileItemRepository.Where(s => s.IsActive).ToListAsync();
            var fileDtos = _mapper.Map<List<FileItemDto>>(files);

            
            if (!User.IsInRole("Admin"))
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                fileDtos = fileDtos.Where(s => s.AppUserId == userId).ToList();
            }

            return fileDtos;
        }

        [HttpGet("{id}")]
        public async Task<FileItemDto> GetById(int id)
        {
            var file = await _fileItemRepository.GetByIdAsync(id);
            var fileDto = _mapper.Map<FileItemDto>(file);
            return fileDto;
        }

        
        [Route("Upload")]
        [HttpPost]
        public async Task<ResultDto> Upload(UploadDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _result.Status = false;
                _result.Message = "Kullanıcı bulunamadı!";
                return _result;
            }

            string data = dto.FileData;
            string base64 = data.Substring(data.IndexOf(',') + 1).Trim('\0');
            byte[] fileBytes = Convert.FromBase64String(base64);

            double fileSizeMB = fileBytes.Length / (1024.0 * 1024.0);
            long fileSizeMBLong = (long)Math.Ceiling(fileSizeMB);

            if (user.UsedStorage + fileSizeMBLong > user.StorageQuota)
            {
                _result.Status = false;
                _result.Message = $"Kota Yetersiz! Dosya boyutu {fileSizeMB:F2} MB. Kalan kotanız: {user.StorageQuota - user.UsedStorage} MB";
                return _result;
            }

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/Files/UserUploads");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string systemFileName = Guid.NewGuid().ToString() + dto.FileExt;
            var fullPath = Path.Combine(path, systemFileName);
            System.IO.File.WriteAllBytes(fullPath, fileBytes);

            var fileItem = new FileItem
            {
                Name = "Yeni_Dosya" + dto.FileExt,
                SystemFileName = systemFileName,
                Extension = dto.FileExt,
                SizeInMB = fileSizeMB,
                FolderId = dto.FolderId,
                AppUserId = userId,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                IsActive = true
            };
            await _fileItemRepository.AddAsync(fileItem);

            user.UsedStorage += fileSizeMBLong;
            await _userManager.UpdateAsync(user);

            _result.Status = true;
            _result.Message = "Dosya başarıyla yüklendi.";
            return _result;
        }

        
        [HttpPut]
        public async Task<ResultDto> Update(FileItemDto model)
        {
            var file = await _fileItemRepository.GetByIdAsync(model.Id);
            if (file == null)
            {
                _result.Status = false;
                _result.Message = "Dosya bulunamadı!";
                return _result;
            }

            file.Name = model.Name;
            file.IsActive = model.IsActive;
            file.Updated = DateTime.Now;

            await _fileItemRepository.UpdateAsync(file);
            _result.Status = true;
            _result.Message = "Dosya Bilgileri Güncellendi";
            return _result;
        }

        
        [HttpDelete("{id}")]
        public async Task<ResultDto> Delete(int id)
        {
            var file = await _fileItemRepository.GetByIdAsync(id);
            if (file != null)
            {
                var user = await _userManager.FindByIdAsync(file.AppUserId);
                if (user != null)
                {
                    user.UsedStorage = Math.Max(0, user.UsedStorage - (long)Math.Ceiling(file.SizeInMB));
                    await _userManager.UpdateAsync(user);
                }

                await _fileItemRepository.DeleteAsync(id); 
                _result.Status = true;
                _result.Message = "Dosya silindi ve kota alanınız iade edildi.";
            }
            return _result;
        }

        //ÇÖP KUTUSU YÖNETİMİ

        [HttpGet("DeletedFiles")]
        [Authorize(Roles = "Admin")]
        public async Task<List<FileItemDto>> DeletedFiles()
        {
            var files = await _fileItemRepository.Where(s => !s.IsActive).ToListAsync();
            return _mapper.Map<List<FileItemDto>>(files);
        }

        [HttpPost("Restore/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ResultDto> Restore(int id)
        {
            var file = await _fileItemRepository.GetByIdAsync(id);
            if (file == null)
            {
                _result.Status = false;
                _result.Message = "Dosya bulunamadı!";
                return _result;
            }

            var user = await _userManager.FindByIdAsync(file.AppUserId);

            if (user.UsedStorage + (long)Math.Ceiling(file.SizeInMB) > user.StorageQuota)
            {
                _result.Status = false;
                _result.Message = $"Hata: Kullanıcının kotası dolu! Dosya geri yüklenemez. (Dosya: {file.SizeInMB:F2} MB)";
                return _result;
            }

            file.IsActive = true;
            file.Updated = DateTime.Now;
            user.UsedStorage += (long)Math.Ceiling(file.SizeInMB);

            await _fileItemRepository.UpdateAsync(file);
            await _userManager.UpdateAsync(user);

            _result.Status = true;
            _result.Message = "Dosya başarıyla geri yüklendi ve kullanıcının kotasına yansıtıldı.";
            return _result;
        }

        [HttpDelete("HardDelete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ResultDto> HardDelete(int id)
        {
            var file = await _fileItemRepository.GetByIdAsync(id);
            if (file == null)
            {
                _result.Status = false;
                _result.Message = "Dosya bulunamadı!";
                return _result;
            }

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/Files/UserUploads", file.SystemFileName);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            _fileItemRepository._context.Files.Remove(file);
            await _fileItemRepository._context.SaveChangesAsync();

            _result.Status = true;
            _result.Message = "Dosya sunucudan kalıcı olarak temizlendi.";
            return _result;
        }
    }
}