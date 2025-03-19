using Rapidpay.Data.Models;

namespace Rapidpay.Business.Interfaces;

public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task<Transaction?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<Transaction>> GetTransactionsByCardIdAsync(int cardId);
    Task<Transaction> ProcessTransactionAsync(Transaction transaction);
} 