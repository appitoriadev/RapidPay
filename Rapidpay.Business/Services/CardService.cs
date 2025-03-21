using Microsoft.EntityFrameworkCore;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data;
using Rapidpay.Data.Interfaces;
using Rapidpay.Data.Models;

namespace Rapidpay.Business.Services;

public class CardService : ICardService
{
    private readonly IUnitOfWork _unitOfWork;

    public CardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Card> CreateCardAsync(Card card)
    {
        var existingCard = await _unitOfWork.CardRepository.GetAllAsync(c => c.CardNumber == card.CardNumber);
        if (existingCard.Any())
            throw new InvalidOperationException("Card already exists");

        await _unitOfWork.CardRepository.AddAsync(card);
        return card;
    }


    public async Task<Card?> GetCardByIdAsync(int id)
    {
        var card = await _unitOfWork.CardRepository.GetAllAsync(c => c.Id == id);
        if (!card.Any())
            throw new InvalidOperationException("Card doesn't exist");

        return card.FirstOrDefault();
    }

    public async Task<Card?> GetCardByNumberAsync(string cardNumber)
    {
        var card = await _unitOfWork.CardRepository.GetAllAsync(c => c.CardNumber == cardNumber);
        if (!card.Any())
            throw new InvalidOperationException("Card doesn't exist");

        return card.FirstOrDefault();
    }

    public async Task<IEnumerable<Card>> GetCardsByUserIdAsync(int userId)
    {
        var cards = await _unitOfWork.CardRepository.GetAllAsync(c => c.UserId == userId);
        if (!cards.Any())
            throw new InvalidOperationException("Card doesn't exist");

        return cards;
    }

    public async Task<bool> ValidateCardAsync(string cardNumber, string expiryMonth, string expiryYear, string cvv)
    {
        var card = await GetCardByNumberAsync(cardNumber);
        if (card == null) return false;

        return card.ExpiryMonth == expiryMonth &&
               card.ExpiryYear == expiryYear &&
               card.Cvv == cvv;
    }

    public async Task<decimal> GetCardBalanceAsync(string cardNumber)
    {
        var card = await GetCardByNumberAsync(cardNumber);
        if (card == null) return 0;

        var pendingTransactions = await _unitOfWork.TransactionRepository.GetAllAsync(t => t.CardId == card.Id && t.Status == TransactionStatus.Pending);

        //  card.Balance
        var totalPendingAmount = pendingTransactions.Sum(t => t.Amount);
        var totalTransactions = await _unitOfWork.TransactionRepository.GetAllAsync(t => t.CardId == card.Id);
        var totalTransactionsAmount = totalTransactions.Sum(t => t.Amount);

        var balance = card.Balance - totalTransactionsAmount - totalPendingAmount;
        if (balance < 0) return 0;
        return balance;
    }
}