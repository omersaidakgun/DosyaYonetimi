namespace DosyaYonetimi.API.DTOs
{
    public class BaseDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}