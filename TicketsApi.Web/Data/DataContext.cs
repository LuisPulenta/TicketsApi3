using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketsApi.Web.Data.Entities;

namespace TicketsApi.Web.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        
        public DbSet<Company> Companies { get; set; }
        public DbSet<TicketCab> TicketCabs { get; set; }
        public DbSet<TicketDet> TicketDets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Company>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Subcategory>().HasIndex("Name", "CategoryId").IsUnique();
        }
    }
}