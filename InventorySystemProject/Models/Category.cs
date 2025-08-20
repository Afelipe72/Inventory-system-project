namespace InventorySystemProject.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        // one-to-many
        // one side
        public List<Item>? Items { get; set; }

    }
}
