namespace DosyaYonetimi.API.DTOs
{
    
    public class FavoriteAddDto
    {
        public int? FileItemId { get; set; }
        public int? FolderId { get; set; }
    }

   
    public class FavoriteDto : BaseDto
    {
        public string AppUserId { get; set; }
        public int? FileItemId { get; set; }
        public int? FolderId { get; set; }
    }
}