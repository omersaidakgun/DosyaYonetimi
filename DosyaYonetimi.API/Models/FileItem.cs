namespace DosyaYonetimi.API.Models
{
    public class FileItem : BaseEntity
    {
       
        public string Name { get; set; }

        
        public string SystemFileName { get; set; }

        
        public string Extension { get; set; }

        
        public double SizeInMB { get; set; }

        
        public int? FolderId { get; set; }
        public Folder Folder { get; set; }

        
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}