using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    public class FolderController : ControllerBase
    {
        private readonly FolderRepository _folderRepository;
        private readonly FileItemRepository _fileItemRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();

        public FolderController(IMapper mapper, FolderRepository folderRepository, FileItemRepository fileItemRepository)
        {
            _mapper = mapper;
            _folderRepository = folderRepository;
            _fileItemRepository = fileItemRepository;
        }

        [HttpGet]
        public async Task<List<FolderDto>> List()
        {
            var folders = await _folderRepository.GetAllAsync();
            var folderDtos = _mapper.Map<List<FolderDto>>(folders);

            
            if (!User.IsInRole("Admin"))
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                folderDtos = folderDtos.Where(s => s.AppUserId == userId).ToList();
            }

            return folderDtos;
        }

        [HttpGet("{id}")]
        public async Task<FolderDto> GetById(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);
            var folderDto = _mapper.Map<FolderDto>(folder);
            return folderDto;
        }

        
        [HttpGet("{id}/Files")]
        public async Task<List<FileItemDto>> FileList(int id)
        {
            
            var files = await _fileItemRepository.Where(s => s.FolderId == id && s.IsActive).ToListAsync();
            var fileDtos = _mapper.Map<List<FileItemDto>>(files);

            
            if (!User.IsInRole("Admin"))
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                fileDtos = fileDtos.Where(s => s.AppUserId == userId).ToList();
            }

            return fileDtos;
        }

        [HttpPost]
        public async Task<ResultDto> Add(FolderDto model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            
            var list = _folderRepository.Where(s => s.Name == model.Name && s.AppUserId == userId).ToList();
            if (list.Count > 0)
            {
                _result.Status = false;
                _result.Message = "Bu isimde bir klasörünüz zaten var!";
                return _result;
            }

            var folder = _mapper.Map<Folder>(model);
            folder.AppUserId = userId; 
            folder.Created = DateTime.Now;
            folder.Updated = DateTime.Now;
            folder.IsActive = true;

            await _folderRepository.AddAsync(folder);
            _result.Status = true;
            _result.Message = "Klasör Başarıyla Oluşturuldu";
            return _result;
        }

        [HttpPut]
        public async Task<ResultDto> Update(Folder model)
        {
            var folder = await _folderRepository.GetByIdAsync(model.Id);
            if (folder == null)
            {
                _result.Status = false;
                _result.Message = "Klasör bulunamadı!";
                return _result;
            }

            folder.Name = model.Name;
            folder.IsActive = model.IsActive;
            folder.Updated = DateTime.Now;

            await _folderRepository.UpdateAsync(folder);
            _result.Status = true;
            _result.Message = "Klasör Adı Güncellendi";
            return _result;
        }

        [HttpDelete("{id}")]
        public async Task<ResultDto> Delete(int id)
        {
            
            var list = await _fileItemRepository.Where(s => s.FolderId == id && s.IsActive).ToListAsync();
            if (list.Count > 0)
            {
                _result.Status = false;
                _result.Message = "İçinde dosya bulunan klasör silinemez! Lütfen önce içindeki dosyaları silin.";
                return _result;
            }

            
            await _folderRepository.DeleteAsync(id);
            _result.Status = true;
            _result.Message = "Klasör Silindi (Çöp Kutusuna Taşındı)";
            return _result;
        }
    }
}