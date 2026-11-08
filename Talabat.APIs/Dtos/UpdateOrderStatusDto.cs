using System;
using System.ComponentModel.DataAnnotations;

namespace Talabat.APIs.Dtos
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; }

        public bool ForceManualOverride { get; set; } = false;
    }

    public class StripeVerificationDto
    {
        public int OrderId { get; set; }
        public string PaymentIntentId { get; set; }
        public string StripeStatus { get; set; }
        public bool IsVerifiedPaid { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string ErrorMessage { get; set; }
        public DateTimeOffset VerifiedAt { get; set; } = DateTimeOffset.Now;
        public OrderToReturnDto Order { get; set; }
    }
}
