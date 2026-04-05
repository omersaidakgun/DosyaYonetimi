namespace DosyaYonetimi.API.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhotoUrl { get; set; }

        
        public long StorageQuota { get; set; }
        public long UsedStorage { get; set; }
    }
}