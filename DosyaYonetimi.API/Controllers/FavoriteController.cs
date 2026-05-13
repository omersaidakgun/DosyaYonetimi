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
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteRepository _favoriteRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();

        public FavoriteController(FavoriteRepository favoriteRepository, IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<List<FavoriteDto>> List()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var favorites = await _favoriteRepository.Where(s => s.IsActive && s.AppUserId == userId).ToListAsync();
            var favoriteDtos = _mapper.Map<List<FavoriteDto>>(favorites);

            return favoriteDtos;
        }

        [HttpPost("AddFavorite")]
        public async Task<ResultDto> Add(FavoriteAddDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            
            var existingFavorite = await _favoriteRepository
                .Where(f => f.AppUserId == userId && f.FileItemId == dto.FileItemId && f.IsActive)
                .FirstOrDefaultAsync();

            if (existingFavorite != null)
            {
                
                _favoriteRepository._context.Favorites.Remove(existingFavorite);
                await _favoriteRepository._context.SaveChangesAsync();

                _result.Status = true;
                _result.Message = "Favorilerden çıkarıldı.";
                return _result;
            }

            
            var favorite = new Favorite
            {
                AppUserId = userId,
                FileItemId = dto.FileItemId,
                FolderId = dto.FolderId,
                Created = DateTime.Now,
                IsActive = true
            };

            await _favoriteRepository.AddAsync(favorite);
            _result.Status = true;
            _result.Message = "Favorilere eklendi.";
            return _result;
        }
    }
}