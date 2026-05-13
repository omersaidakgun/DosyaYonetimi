namespace DosyaYonetimi.UI.Models
{
    public class AdminSummaryViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalActiveFiles { get; set; }
        public int TotalActiveFolders { get; set; }
        public int TotalFilesInTrash { get; set; }
    }
}