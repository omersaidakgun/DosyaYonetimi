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
    public class FileShareController : ControllerBase
    {
        private readonly FileShareRepository _fileShareRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();

        public FileShareController(FileShareRepository fileShareRepository, IMapper mapper)
        {
            _fileShareRepository = fileShareRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<List<FileShareDto>> List()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var shares = await _fileShareRepository.Where(s => s.IsActive && (s.OwnerUserId == userId || s.SharedWithUserId == userId)).ToListAsync();
            var shareDtos = _mapper.Map<List<FileShareDto>>(shares);

            return shareDtos;
        }

        [HttpPost("CreateShare")]
        public async Task<ResultDto> CreateShare(FileShareAddDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var share = new DosyaYonetimi.API.Models.FileShare
            {
                FileItemId = dto.FileItemId,
                OwnerUserId = userId,
                SharedWithUserId = dto.SharedWithUserId,
                CanEdit = dto.CanEdit,
                ExpiryDate = DateTime.Now.AddDays(dto.ExpiryDays),
                Created = DateTime.Now,
                IsActive = true
            };

            await _fileShareRepository.AddAsync(share);

            _result.Status = true;
            _result.Message = "Dosya paylaşım linki başarıyla oluşturuldu.";
            return _result;
        }
    }
}