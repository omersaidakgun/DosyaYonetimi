using AutoMapper;
using DosyaYonetimi.API.DTOs;
using DosyaYonetimi.API.Models;

namespace DosyaYonetimi.API.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            
            CreateMap<FileItem, FileItemDto>().ReverseMap();
            CreateMap<Folder, FolderDto>().ReverseMap();

             
            CreateMap<AppUser, UserDto>().ReverseMap();

            
            
             
        }
    }
}