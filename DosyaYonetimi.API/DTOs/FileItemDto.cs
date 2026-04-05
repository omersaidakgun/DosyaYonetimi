namespace DosyaYonetimi.API.DTOs
{
    public class FileItemDto : BaseDto
    {
        public string Name { get; set; }
        public string SystemFileName { get; set; }
        public string Extension { get; set; }
        public double SizeInMB { get; set; }
        public int? FolderId { get; set; }
        public string AppUserId { get; set; }
    }
}