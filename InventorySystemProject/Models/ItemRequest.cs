using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystemProject.Models
{
    public class ItemRequest
    {

        //Id(primary key)
        public int Id { get; set; }
        //ItemId(foreign key to your Item table)
        public int? ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item? Items { get; set; }
        //UserId(foreign key to your AspNetUsers or ApplicationUser table)
        [BindNever]
        public string? ApplicationUserId { get; set; } = null!;
        [BindNever]
        public ApplicationUser? ApplicationUsers { get; set; }        
        //Quantity(optional, if users request more than one)
        public int Quantity { get; set; }
        //Status(Pending, Approved, Rejected)
        public string Status { get; set; } = "Pending";
        //RequestDate
        public DateTime RequestDate { get; set; }

    }
}
