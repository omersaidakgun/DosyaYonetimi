using DosyaYonetimi.API.Models;

namespace DosyaYonetimi.API.Repositories
{
    
    public class FileShareRepository : GenericRepository<DosyaYonetimi.API.Models.FileShare>
    {
        public FileShareRepository(AppDbContext context) : base(context)
        {
        }
    }
}