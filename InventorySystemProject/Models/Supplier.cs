namespace InventorySystemProject.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ContactInfo { get; set; } = null!;
        // one-to-many
        // one side
        public List<Item>? Items { get; set; }

    }
}
