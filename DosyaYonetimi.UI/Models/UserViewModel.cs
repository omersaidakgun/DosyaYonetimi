namespace DosyaYonetimi.UI.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public long StorageQuota { get; set; }
        public long UsedStorage { get; set; }
    }
}