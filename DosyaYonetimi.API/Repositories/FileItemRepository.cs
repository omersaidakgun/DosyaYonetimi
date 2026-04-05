using DosyaYonetimi.API.Models;

namespace DosyaYonetimi.API.Repositories
{
    public class FileItemRepository : GenericRepository<FileItem>
    {
        public FileItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}