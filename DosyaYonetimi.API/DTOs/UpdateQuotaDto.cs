namespace DosyaYonetimi.API.DTOs
{
    public class UpdateQuotaDto
    {
        public string UserId { get; set; }
        public long NewStorageQuota { get; set; }
    }
}