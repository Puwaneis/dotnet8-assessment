using System.ComponentModel.DataAnnotations;

namespace HLIBAssessment.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string TransactionReferenceNumber { get; set; } = string.Empty;
        public string CustomerUsername { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public Customer Customer { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
