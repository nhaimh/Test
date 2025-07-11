namespace Test.DTO
{
    public class MonthlyReportDto
    {
        public string MonthYear { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public string TopSaleProduct { get; set; }
        public double GrowthRate { get; set; }
    }
}
