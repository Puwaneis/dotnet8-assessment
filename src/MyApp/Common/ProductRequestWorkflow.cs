using MyApp.Model;

namespace MyApp.Common;

public static class ProductRequestWorkflow
{
    /// <summary>
    /// Validates whether the approval can be applied.
    /// Returns (true, null) if valid, or (false, errorMessage) if not.
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) ValidateApproval(ProductRequest request)
    {
        if (request.RequestType == RequestType.Remove &&
            request.Product.Quantity < request.RequestedQuantity)
        {
            return (false, "Insufficient quantity");
        }
        return (true, null);
    }

    /// <summary>
    /// Applies the quantity change to the product.
    /// Call only after ValidateApproval returns IsValid = true.
    /// </summary>
    public static void ApplyQuantityChange(ProductRequest request, Product product)
    {
        if (request.RequestType == RequestType.Add)
            product.Quantity += request.RequestedQuantity;
        else
            product.Quantity -= request.RequestedQuantity;

        product.UpdatedAt = DateTime.UtcNow;
    }
}
