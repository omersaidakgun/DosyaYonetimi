namespace DosyaYonetimi.API.Models
{
    public class FileShare : BaseEntity
    {
        public int FileItemId { get; set; }
        public FileItem FileItem { get; set; }

        
        public string OwnerUserId { get; set; }

        
        public string SharedWithUserId { get; set; }

        public bool CanEdit { get; set; } 
        public DateTime ExpiryDate { get; set; } 
    }
}