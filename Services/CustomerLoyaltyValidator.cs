using System;
using BarterPOS.Models;

namespace BarterPOS.Services
{
    public static class CustomerLoyaltyValidator
    {
        public static bool ValidateCustomer(Customer? customer, out string message)
        {
            if (customer == null)
            {
                message = "Enter a valid customer loyalty ID.";
                return false;
            }

            if (customer.Id <= 0 || string.IsNullOrWhiteSpace(customer.Name))
            {
                message = "The customer loyalty record is incomplete.";
                return false;
            }

            if (!customer.IsActive)
            {
                message = "This loyalty account is inactive and cannot be used.";
                return false;
            }

            string type = NormalizeType(customer.Type);
            if (type is not "REGULAR" and not "PWD" and not "SENIOR")
            {
                message = "This loyalty account has an unsupported customer type.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool ValidateDiscountEligibility(
            Customer? customer,
            bool isPwdDiscount,
            bool isSeniorDiscount,
            out string message)
        {
            if (isPwdDiscount && isSeniorDiscount)
            {
                message = "Only one customer discount may be applied per transaction.";
                return false;
            }

            if (!isPwdDiscount && !isSeniorDiscount)
            {
                message = string.Empty;
                return true;
            }

            message = string.Empty;
            return true;
        }

        private static string NormalizeType(string customerType) =>
            customerType.Trim().ToUpperInvariant();
    }
}
