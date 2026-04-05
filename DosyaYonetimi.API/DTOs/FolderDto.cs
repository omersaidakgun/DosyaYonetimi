namespace DosyaYonetimi.API.DTOs
{
    public class FolderDto : BaseDto
    {
        public string Name { get; set; }
        public string AppUserId { get; set; }
        public int? ParentFolderId { get; set; }
    }
}