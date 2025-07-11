namespace Test.DTO
{
    public class LowStockDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int StockQtty { get; set; }
        public double AvgSoldLast3Months { get; set; }
        public double ExpectedShortage { get; set; }
    }
}
