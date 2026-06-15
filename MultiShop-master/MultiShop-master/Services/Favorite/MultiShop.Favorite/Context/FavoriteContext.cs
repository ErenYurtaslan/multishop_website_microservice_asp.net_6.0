using Microsoft.EntityFrameworkCore;
using MultiShop.Favorite.Entities;

namespace MultiShop.Favorite.Context
{
    public class FavoriteContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Initial Catalog=MultiShopFavoriteDb;Integrated Security=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserFavorite>()
                .HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();
        }

        public DbSet<UserFavorite> UserFavorites { get; set; } = null!;
    }
}
