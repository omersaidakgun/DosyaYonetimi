using DosyaYonetimi.API.Models;

namespace DosyaYonetimi.API.Repositories
{
    public class FavoriteRepository : GenericRepository<Favorite>
    {
        public FavoriteRepository(AppDbContext context) : base(context)
        {
        }
    }
}