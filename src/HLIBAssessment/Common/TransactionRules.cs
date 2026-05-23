using HLIBAssessment.Model;

namespace HLIBAssessment.Common
{
    public class TransactionRules
    {
        public static (bool IsValid, string? ErrorMessage) ValidateQuantity(Product product, int quantity)
        {
            if (quantity <= 0)
                return (false, "Quantity must be at least 1.");

            if (quantity > product.Quantity)
                return (false, $"Cannot place order more than available items.");

            return (true, null);
        }

        public static decimal CalculateTotalPrice(Product product, int quantity)
        {
            return product.Price * quantity;
        }

        public static string GenerateReferenceNumber()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return $"TXN-{datePart}-{randomPart}";
        }

        public static void ApplyPurchase(Product product, int quantity)
        {
            product.Quantity -= quantity;
            product.UpdatedAt = DateTime.UtcNow;
        }
    }
}
