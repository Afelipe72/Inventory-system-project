using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystemProject.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public double Price { get; set; }

        // many-to-one
        // many side
        public int? CategoryId { get; set; }
        // each item belongs to one category, and CategoryId holds the ID of that category
        [ForeignKey("CategoryId")]
        public Category? Categories { get; set; }

        // many-to-one
        // many side
        public int? SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        public int QuantityInStock { get; set; }

        public List<ItemRequest>? ItemRequests { get; set; }



    }
}
