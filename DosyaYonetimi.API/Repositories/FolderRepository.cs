using DosyaYonetimi.API.Models;

namespace DosyaYonetimi.API.Repositories
{
    public class FolderRepository : GenericRepository<Folder>
    {
        public FolderRepository(AppDbContext context) : base(context)
        {
        }
    }
}