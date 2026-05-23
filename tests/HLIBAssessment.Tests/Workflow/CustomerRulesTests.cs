using HLIBAssessment.Common;
using HLIBAssessment.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLIBAssessment.Tests.Workflow
{
    public class CustomerRulesTests
    {
        private static Customer ActiveCustomer() => new()
        {
            Id = Guid.Parse("a17c3485-a401-45e4-a93f-40a8337870f7"),
            Username = "Acme",
            Status = true
        };

        [Fact]
        public void ValidateDeactivation_NoOpenOrders_IsValid()
        {
            var customer = ActiveCustomer();
            var (isValid, error) = CustomerRules.ValidateDeactivation(customer, 0);
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Fact]
        public void ValidateDeactivation_ExactlyOneOpenOrder_IsInvalid()
        {
            var customer = ActiveCustomer();
            var (isValid, error) = CustomerRules.ValidateDeactivation(customer, 1);
            Assert.False(isValid);
            Assert.Equal("Cannot deactivate customer with 1 open order(s).", error);
        }

        [Fact]
        public void ValidateDeactivation_HasOpenOrders_IsInvalid()
        {
            var customer = ActiveCustomer();
            var (isValid, error) = CustomerRules.ValidateDeactivation(customer, 3);
            Assert.False(isValid);
            Assert.Equal("Cannot deactivate customer with 3 open order(s).", error);
        }

        [Fact]
        public void ValidateDeletion_NoOpenOrders_IsValid()
        {
            var customer = ActiveCustomer();
            var (isValid, error) = CustomerRules.ValidateDeletion(customer, 0);
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Fact]
        public void ValidateDeletion_ExactlyOneOpenOrder_IsInvalid()
        {
            var customer = ActiveCustomer();
            var (isValid, error) = CustomerRules.ValidateDeletion(customer, 1);
            Assert.False(isValid);
            Assert.Equal("Cannot delete customer with 1 open order(s).", error);
        }

        [Fact]
        public void ValidateDeletion_HasOpenOrders_IsInvalid()
        {
            var customer = ActiveCustomer();
            var (isValid, error) = CustomerRules.ValidateDeletion(customer, 2);
            Assert.False(isValid);
            Assert.NotNull(error);
        }

        [Fact]
        public void ValidateCreate_ValidCustomer_IsValid()
        {
            var customer = new Customer
            {
                Username = "Acme Corp",
                Email = "acme@example.com",
                PhoneNumber = "0123456789"
            };
            var (isValid, error, _) = CustomerRules.ValidateCreate(customer);
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Fact]
        public void ValidateCreate_EmptyUsername_IsInvalid()
        {
            var customer = new Customer { Username = "   " };
            var (isValid, error, _) = CustomerRules.ValidateCreate(customer);
            Assert.False(isValid);
            Assert.Equal("Username is required.", error);
        }

        [Fact]
        public void ValidateCreate_UsernameAtMaxLength_IsValid()
        {
            var customer = new Customer { Username = new string('A', 50) };
            var (isValid, error, _) = CustomerRules.ValidateCreate(customer);
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Fact]
        public void ValidateCreate_UsernameTooLong_IsInvalid()
        {
            var customer = new Customer { Username = new string('A', 51) };
            var (isValid, error, _) = CustomerRules.ValidateCreate(customer);
            Assert.False(isValid);
            Assert.Equal("Username cannot exceed 50 characters.", error);
        }

        [Fact]
        public void ValidateCanPurchase_ActiveCustomer_IsValid()
        {
            var customer = ActiveCustomer();
            var (isValid, error) = CustomerRules.ValidateCanPurchase(customer);
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Fact]
        public void ValidateCanPurchase_InactiveCustomer_IsInvalid()
        {
            var customer = ActiveCustomer();
            customer.Status = false;
            var (isValid, error) = CustomerRules.ValidateCanPurchase(customer);
            Assert.False(isValid);
            Assert.Equal("Inactive customers cannot make purchases.", error);
        }

        [Fact]
        public void ValidateCanPurchase_NewCustomerDefaultStatus_IsValid()
        {
            var customer = new Customer { Username = "NewUser" };
            var (isValid, error) = CustomerRules.ValidateCanPurchase(customer);
            Assert.True(isValid);
            Assert.Null(error);
        }

        [Fact]
        public void ApplyDeactivation_SetsStatusToFalse()
        {
            var customer = ActiveCustomer();
            CustomerRules.ApplyDeactivation(customer);
            Assert.False(customer.Status);
        }

        [Fact]
        public void ApplyDeactivation_UpdatesUpdatedAt()
        {
            var before = DateTime.UtcNow.AddDays(-1);
            var customer = ActiveCustomer();
            customer.UpdatedAt = before;
            CustomerRules.ApplyDeactivation(customer);
            Assert.True(customer.UpdatedAt > before);
        }
    }
}
