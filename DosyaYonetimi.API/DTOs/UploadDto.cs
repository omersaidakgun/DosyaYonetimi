namespace DosyaYonetimi.API.DTOs
{
    public class UploadDto
    {
        public string FileName { get; set; }
        public string FileData { get; set; } 
        public string FileExt { get; set; }  
        public int? FolderId { get; set; }   
    }
}