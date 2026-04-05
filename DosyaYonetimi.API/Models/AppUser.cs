using Microsoft.AspNetCore.Identity;

namespace DosyaYonetimi.API.Models 
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }

        
        public long StorageQuota { get; set; }

        
        public long UsedStorage { get; set; }
    }
}