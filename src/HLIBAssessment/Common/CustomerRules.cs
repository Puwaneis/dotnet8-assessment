using HLIBAssessment.Model;

namespace HLIBAssessment.Common
{
    public static class CustomerRules
    {
        public static (bool IsValid, string? ErrorMessage) ValidateDeactivation(Customer customer, int openOrderCount)
        {
            if (openOrderCount > 0)
                return (false, $"Cannot deactivate customer with {openOrderCount} open order(s).");

            return (true, null);
        }

        public static (bool IsValid, string? ErrorMessage) ValidateDeletion(Customer customer, int openOrderCount)
        {
            if (openOrderCount > 0)
                return (false, $"Cannot delete customer with {openOrderCount} open order(s).");

            return (true, null);
        }

        public static (bool IsValid, string? ErrorMessage, string? FieldName) ValidateCreate(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Username))
                return (false, "Username is required.", nameof(Customer.Username));

            if (customer.Username.Length > 50)
                return (false, "Username cannot exceed 50 characters.", nameof(Customer.Username));

            if (customer.Email?.Length > 100)
                return (false, "Email cannot exceed 100 characters.", nameof(Customer.Email));

            if (customer.PhoneNumber?.Length > 20)
                return (false, "Phone number cannot exceed 20 characters.", nameof(Customer.PhoneNumber));

            return (true, null, null);
        }

        public static (bool IsValid, string? ErrorMessage) ValidateCanPurchase(Customer customer)
        {
            if (!customer.Status)
                return (false, "Inactive customers cannot make purchases.");

            return (true, null);
        }

        public static void ApplyDeactivation(Customer customer)
        {
            customer.Status = false;
            customer.UpdatedAt = DateTime.UtcNow;
        }
    }
}
