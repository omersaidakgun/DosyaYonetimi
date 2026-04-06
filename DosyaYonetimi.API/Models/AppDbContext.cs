using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DosyaYonetimi.API.Models 
{
    
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        
        public DbSet<Folder> Folders { get; set; }
        public DbSet<FileItem> Files { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<FileShare> FileShares { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}