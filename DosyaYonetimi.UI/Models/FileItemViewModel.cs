namespace DosyaYonetimi.UI.Models
{
    public class FileItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemFileName { get; set; } 
        public string Extension { get; set; }
        public double SizeInMB { get; set; }
        public DateTime Created { get; set; }
        public bool IsFavorite { get; set; }
        public int? FolderId { get; set; }
    }
}