namespace DosyaYonetimi.API.DTOs
{
    
    public class FileShareAddDto
    {
        public int FileItemId { get; set; }
        public string SharedWithUserId { get; set; }
        public bool CanEdit { get; set; }
        public int ExpiryDays { get; set; }
    }

    
    public class FileShareDto : BaseDto
    {
        public int FileItemId { get; set; }
        public string OwnerUserId { get; set; }
        public string SharedWithUserId { get; set; }
        public bool CanEdit { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}