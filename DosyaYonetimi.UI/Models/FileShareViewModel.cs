using System;

namespace DosyaYonetimi.UI.Models
{
    public class FileShareViewModel
    {
        public int Id { get; set; }
        public int FileItemId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }

        
        public DateTime EndDate { get; set; }

        
        public string? FileItemName { get; set; }
        public string? SenderUserName { get; set; }
    }
}