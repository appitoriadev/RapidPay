using Microsoft.EntityFrameworkCore;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data;
using Rapidpay.Data.Models;

namespace Rapidpay.Business.Services;

public class TransactionService : ITransactionService
{
    private readonly RapidpayDbContext _context;
    private readonly ICardService _cardService;

    public TransactionService(RapidpayDbContext context, ICardService cardService)
    {
        _context = context;
        _cardService = cardService;
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        transaction.Status = TransactionStatus.Pending;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByCardIdAsync(int cardId)
    {
        return await _context.Transactions
            .Where(t => t.CardId == cardId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction> ProcessTransactionAsync(Transaction transaction)
    {
        // Get card with lock
        var card = await _cardService.GetCardByIdAsync(transaction.CardId);
        if (card == null)
            throw new InvalidOperationException("Card not found");

        // Process transaction
        transaction.Status = TransactionStatus.Completed;
        transaction.CompletedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return transaction;
    }
} 