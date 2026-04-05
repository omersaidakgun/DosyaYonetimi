using Microsoft.AspNetCore.Mvc.Filters;

namespace DosyaYonetimi.API.Models
{
    public class Folder : BaseEntity
    {
        
        public string Name { get; set; }

        
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        
        public int? ParentFolderId { get; set; }
        public Folder ParentFolder { get; set; }

        
        public ICollection<FileItem> Files { get; set; }

        
        public ICollection<Folder> SubFolders { get; set; }
    }
}