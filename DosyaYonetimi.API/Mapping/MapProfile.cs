using AutoMapper;
using DosyaYonetimi.API.DTOs;
using DosyaYonetimi.API.Models;
using FileShare = DosyaYonetimi.API.Models.FileShare;

namespace DosyaYonetimi.API.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {

            CreateMap<FileItem, FileItemDto>().ReverseMap();
            CreateMap<Folder, FolderDto>().ReverseMap();
            CreateMap<AppUser, UserDto>().ReverseMap();


            CreateMap<Favorite, FavoriteDto>().ReverseMap();
            CreateMap<Favorite, FavoriteAddDto>().ReverseMap();


            CreateMap<FileShare, FileShareDto>().ReverseMap();
            CreateMap<FileShare, FileShareAddDto>().ReverseMap();
        }
    }
}