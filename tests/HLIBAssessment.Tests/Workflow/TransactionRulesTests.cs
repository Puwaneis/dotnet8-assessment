using HLIBAssessment.Common;
using HLIBAssessment.Model;

namespace HLIBAssessment.Tests.Workflow;

public class TransactionRulesTests
{
    private static Product ProductWithStock(int quantity, decimal price = 10m) => new()
    {
        Id = 1,
        Name = "Widget A",
        Price = price,
        Quantity = quantity,
        UpdatedAt = DateTime.UtcNow.AddDays(-1)
    };

    [Fact]
    public void ValidateQuantity_WithinStock_IsValid()
    {
        var product = ProductWithStock(10);
        var (isValid, error) = TransactionRules.ValidateQuantity(product, 5);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateQuantity_ExactStock_IsValid()
    {
        var product = ProductWithStock(5);
        var (isValid, error) = TransactionRules.ValidateQuantity(product, 5);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateQuantity_ExceedsStock_IsInvalid()
    {
        var product = ProductWithStock(5);
        var (isValid, error) = TransactionRules.ValidateQuantity(product, 10);

        Assert.False(isValid);
        Assert.Equal("Cannot place order more than available items.", error);
    }

    [Fact]
    public void ValidateQuantity_OneOverStock_IsInvalid()
    {
        var product = ProductWithStock(5);
        var (isValid, error) = TransactionRules.ValidateQuantity(product, 6);

        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidateQuantity_ZeroQuantity_IsInvalid()
    {
        var product = ProductWithStock(10);
        var (isValid, error) = TransactionRules.ValidateQuantity(product, 0);

        Assert.False(isValid);
        Assert.Equal("Quantity must be at least 1.", error);
    }

    [Fact]
    public void CalculateTotalPrice_ReturnsPriceTimesQuantity()
    {
        var product = ProductWithStock(100, price: 9.99m);
        var total = TransactionRules.CalculateTotalPrice(product, 3);
        Assert.Equal(29.97m, total);
    }

    [Fact]
    public void CalculateTotalPrice_SingleItem_ReturnsProductPrice()
    {
        var product = ProductWithStock(10, price: 24.99m);
        var total = TransactionRules.CalculateTotalPrice(product, 1);
        Assert.Equal(24.99m, total);
    }

    [Fact]
    public void ApplyPurchase_DeductsProductQuantity()
    {
        var product = ProductWithStock(10);
        TransactionRules.ApplyPurchase(product, 3);
        Assert.Equal(7, product.Quantity);
    }

    [Fact]
    public void ApplyPurchase_UpdatesProductUpdatedAt()
    {
        var before = DateTime.UtcNow.AddDays(-1);
        var product = ProductWithStock(10);
        product.UpdatedAt = before;

        TransactionRules.ApplyPurchase(product, 1);

        Assert.True(product.UpdatedAt > before);
    }

    [Fact]
    public void GenerateReferenceNumber_HasExpectedFormat()
    {
        var reference = TransactionRules.GenerateReferenceNumber();
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

        Assert.StartsWith($"TXN-{datePart}-", reference);
        Assert.Equal(21, reference.Length); // TXN- + 8 date + - + 8 random
    }

    [Fact]
    public void GenerateReferenceNumber_ProducesUniqueValues()
    {
        var first = TransactionRules.GenerateReferenceNumber();
        var second = TransactionRules.GenerateReferenceNumber();
        Assert.NotEqual(first, second);
    }
}
