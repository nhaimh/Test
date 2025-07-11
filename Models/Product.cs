namespace Test.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int StockQtty { get; set; }
        public decimal Price { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
