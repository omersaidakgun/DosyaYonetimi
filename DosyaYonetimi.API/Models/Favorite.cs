namespace DosyaYonetimi.API.Models
{
    public class Favorite : BaseEntity
    {
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public int? FileItemId { get; set; }
        public FileItem FileItem { get; set; }

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
    }
}