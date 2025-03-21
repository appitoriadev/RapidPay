namespace Rapidpay.API.Models
{
    public class CardValidationRequest
    {
        public required string CardNumber { get; set; }
        public required string ExpiryMonth { get; set; }
        public required string ExpiryYear { get; set; }
        public required string Cvv { get; set; }
    }
}