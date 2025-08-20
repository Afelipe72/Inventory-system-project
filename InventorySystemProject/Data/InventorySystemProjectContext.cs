using Microsoft.EntityFrameworkCore;
using InventorySystemProject.Models;
// for the login
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace InventorySystemProject.Data

{
    public class InventorySystemProjectContext : IdentityDbContext<ApplicationUser>
    {
        
        public InventorySystemProjectContext(DbContextOptions options) : base(options) 
        {
        }

        public DbSet<Category> Categories {  get; set; }
        public DbSet<Item> Items {  get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ItemRequest> ItemRequests { get; set; }

    }



}
