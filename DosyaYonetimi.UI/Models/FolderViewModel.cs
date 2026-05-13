namespace DosyaYonetimi.UI.Models
{
    public class FolderViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AppUserId { get; set; }
        public DateTime Created { get; set; }
        public bool IsActive { get; set; }
        public int? ParentFolderId { get; set; }

    }
}