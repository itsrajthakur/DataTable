namespace DataTable.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public bool IsDeleted { get; set; }
    }
}
