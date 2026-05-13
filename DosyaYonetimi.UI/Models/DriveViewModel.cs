using DosyaYonetimi.UI.Models;

namespace DosyaYonetimi.UI.Models
{
    public class DriveViewModel
    {
        public List<FolderViewModel> Folders { get; set; } = new List<FolderViewModel>();
        public List<FileItemViewModel> Files { get; set; } = new List<FileItemViewModel>();
    }
}