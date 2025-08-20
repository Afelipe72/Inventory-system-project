using Microsoft.AspNetCore.Identity;

namespace InventorySystemProject.Models

{
    // for the login
    public class ApplicationUser : IdentityUser
    {
        public List<ItemRequest>? ItemRequests { get; set; }

    }
}
