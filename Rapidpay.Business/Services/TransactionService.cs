using Microsoft.EntityFrameworkCore;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data;
using Rapidpay.Data.Interfaces;
using Rapidpay.Data.Models;

namespace Rapidpay.Business.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICardService _cardService;

    public TransactionService(IUnitOfWork unitOfWork, ICardService cardService)
    {
        _unitOfWork = unitOfWork;
        _cardService = cardService;
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        transaction.Status = TransactionStatus.Pending;
        await _unitOfWork.TransactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveAsync();
        return transaction;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _unitOfWork.TransactionRepository.FindAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByCardIdAsync(int cardId)
    {
        var transactions = await _unitOfWork.TransactionRepository
            .GetAllAsync(
                filter: t => t.CardId == cardId,
                orderBy: q => q.OrderByDescending(t => t.CreatedAt)
            );
        
        return transactions;
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
        
        await _unitOfWork.SaveAsync();
        return transaction;
    }
} 