namespace Test.DTO
{
    public class OrderDto
    {
        public int CustomerID { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }
}
