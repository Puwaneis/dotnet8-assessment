using MyApp.Common;
using MyApp.Model;

namespace MyApp.Tests.Workflow;

public class ProductRequestWorkflowTests
{
    [Fact]
    public void ValidateApproval_AddRequest_AlwaysValid()
    {
        var product = new Product { Quantity = 5 };
        var request = new ProductRequest
        {
            RequestType = RequestType.Add,
            RequestedQuantity = 100,
            Product = product
        };

        var (isValid, error) = ProductRequestWorkflow.ValidateApproval(request);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateApproval_RemoveRequest_SufficientQuantity_IsValid()
    {
        var product = new Product { Quantity = 10 };
        var request = new ProductRequest
        {
            RequestType = RequestType.Remove,
            RequestedQuantity = 5,
            Product = product
        };

        var (isValid, error) = ProductRequestWorkflow.ValidateApproval(request);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateApproval_RemoveRequest_ExactQuantity_IsValid()
    {
        var product = new Product { Quantity = 5 };
        var request = new ProductRequest
        {
            RequestType = RequestType.Remove,
            RequestedQuantity = 5,
            Product = product
        };

        var (isValid, error) = ProductRequestWorkflow.ValidateApproval(request);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ValidateApproval_RemoveRequest_InsufficientQuantity_IsInvalid()
    {
        var product = new Product { Quantity = 5 };
        var request = new ProductRequest
        {
            RequestType = RequestType.Remove,
            RequestedQuantity = 10,
            Product = product
        };

        var (isValid, error) = ProductRequestWorkflow.ValidateApproval(request);

        Assert.False(isValid);
        Assert.Equal("Insufficient quantity", error);
    }

    [Fact]
    public void ValidateApproval_RemoveRequest_ZeroQuantity_IsInvalid()
    {
        var product = new Product { Quantity = 0 };
        var request = new ProductRequest
        {
            RequestType = RequestType.Remove,
            RequestedQuantity = 1,
            Product = product
        };

        var (isValid, error) = ProductRequestWorkflow.ValidateApproval(request);

        Assert.False(isValid);
        Assert.Equal("Insufficient quantity", error);
    }

    [Fact]
    public void ApplyQuantityChange_AddRequest_IncreasesQuantity()
    {
        var product = new Product { Quantity = 10, UpdatedAt = DateTime.UtcNow.AddDays(-1) };
        var request = new ProductRequest { RequestType = RequestType.Add, RequestedQuantity = 5 };

        ProductRequestWorkflow.ApplyQuantityChange(request, product);

        Assert.Equal(15, product.Quantity);
    }

    [Fact]
    public void ApplyQuantityChange_RemoveRequest_DecreasesQuantity()
    {
        var product = new Product { Quantity = 10, UpdatedAt = DateTime.UtcNow.AddDays(-1) };
        var request = new ProductRequest { RequestType = RequestType.Remove, RequestedQuantity = 3 };

        ProductRequestWorkflow.ApplyQuantityChange(request, product);

        Assert.Equal(7, product.Quantity);
    }

    [Fact]
    public void ApplyQuantityChange_UpdatesProductUpdatedAt()
    {
        var before = DateTime.UtcNow.AddDays(-1);
        var product = new Product { Quantity = 10, UpdatedAt = before };
        var request = new ProductRequest { RequestType = RequestType.Add, RequestedQuantity = 1 };

        ProductRequestWorkflow.ApplyQuantityChange(request, product);

        Assert.True(product.UpdatedAt > before);
    }
}
