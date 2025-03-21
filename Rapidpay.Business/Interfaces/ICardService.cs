using Rapidpay.Data.Models;

namespace Rapidpay.Business.Interfaces;

public interface ICardService
{
    Task<Card> CreateCardAsync(Card card);
    Task<Card?> GetCardByIdAsync(int id);
    Task<Card?> GetCardByNumberAsync(string cardNumber);
    Task<bool> ValidateCardAsync(string cardNumber, string expiryMonth, string expiryYear, string cvv);
    Task<IEnumerable<Card>> GetCardsByUserIdAsync(int userId);
    Task<decimal> GetCardBalanceAsync(string cardNumber);
} 