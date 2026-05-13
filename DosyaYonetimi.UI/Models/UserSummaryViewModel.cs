namespace DosyaYonetimi.UI.Models
{
    public class UserSummaryViewModel
    {
        public string FullName { get; set; }
        public int TotalFolders { get; set; }
        public int TotalFiles { get; set; }

        
        public long UsedStorageMB { get; set; }
        public long RemainingStorageMB { get; set; }
        public long TotalQuotaMB { get; set; }
    }
}