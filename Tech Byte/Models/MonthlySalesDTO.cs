namespace Tech_Byte.Models
{
    public class MonthlySalesDto
    {
        public IdGroup _id { get; set; }
        public decimal TotalSales { get; set; }
        public int Orders { get; set; }
    }

    public class IdGroup
    {
        public int year { get; set; }
        public int month { get; set; }
    }
}
