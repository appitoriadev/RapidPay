using Microsoft.EntityFrameworkCore;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data;
using Rapidpay.Data.Models;

namespace Rapidpay.Business.Services;

public class CardService : ICardService
{
    private readonly RapidpayDbContext _context;

    public CardService(RapidpayDbContext context)
    {
        _context = context;
    }

    public async Task<Card> CreateCardAsync(Card card)
    {
        _context.Cards.Add(card);
        await _context.SaveChangesAsync();
        return card;
    }

    public async Task<Card?> GetCardByIdAsync(int id)
    {
        return await _context.Cards.FindAsync(id);
    }

    public async Task<Card?> GetCardByNumberAsync(string cardNumber)
    {
        return await _context.Cards.FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
    }

    public async Task<IEnumerable<Card>> GetCardsByUserIdAsync(int userId)
    {
        return await _context.Cards
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ValidateCardAsync(string cardNumber, string expiryMonth, string expiryYear, string cvv)
    {
        var card = await GetCardByNumberAsync(cardNumber);
        if (card == null) return false;

        return card.ExpiryMonth == expiryMonth &&
               card.ExpiryYear == expiryYear &&
               card.Cvv == cvv;
    }
} 