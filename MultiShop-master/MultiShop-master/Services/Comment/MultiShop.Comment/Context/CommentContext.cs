using Microsoft.EntityFrameworkCore;
using MultiShop.Comment.Entities;

namespace MultiShop.Comment.Context
{
    public class CommentContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Initial Catalog=MultiShopCommentDb;Integrated Security=True;TrustServerCertificate=True");
        }
        public DbSet<UserComment> UserComments { get; set; }
    }
}
