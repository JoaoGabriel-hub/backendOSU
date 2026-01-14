using Microsoft.EntityFrameworkCore;
using BibliotecaApi.Models;

namespace BibliotecaApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}
