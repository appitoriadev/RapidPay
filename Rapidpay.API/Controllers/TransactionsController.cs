using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data.Models;
using System.Security.Claims;

namespace Rapidpay.API.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICardService _cardService;

    public TransactionsController(ITransactionService transactionService, ICardService cardService)
    {
        _transactionService = transactionService;
        _cardService = cardService;
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new InvalidOperationException("User ID claim not found");
        var userId = int.Parse(userIdClaim);
        
        // Verify that the card belongs to the user
        var card = await _cardService.GetCardByIdAsync(transaction.CardId);
        if (card == null || card.Id != userId)
        {
            return Forbid();
        }

        var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
        return CreatedAtAction(nameof(GetTransaction), new { id = createdTransaction.Id }, createdTransaction);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new InvalidOperationException("User ID claim not found");
        var userId = int.Parse(userIdClaim);
        
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        
        if (transaction == null)
        {
            return NotFound();
        }

        // Verify that the card belongs to the user
        var card = await _cardService.GetCardByIdAsync(transaction.Id);
        if (card == null || card.Id != userId)
        {
            return Forbid();
        }

        return Ok(transaction);
    }

    [HttpGet("card/{cardId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByCard(int cardId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new InvalidOperationException("User ID claim not found");
        var userId = int.Parse(userIdClaim);
        
        // Verify that the card belongs to the user
        var card = await _cardService.GetCardByIdAsync(cardId);
        if (card == null || card.UserId != userId)
        {
            return Forbid();
        }

        var transactions = await _transactionService.GetTransactionsByCardIdAsync(cardId);
        return Ok(transactions);
    }

    [HttpPost("{id}/process")]
    public async Task<ActionResult<Transaction>> ProcessTransaction(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new InvalidOperationException("User ID claim not found");
        var userId = int.Parse(userIdClaim);
        
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        
        if (transaction == null)
        {
            return NotFound();
        }

        // Verify that the card belongs to the user
        var card = await _cardService.GetCardByIdAsync(transaction.CardId);
        if (card == null || card.UserId != userId)
        {
            return Forbid();
        }

        var processedTransaction = await _transactionService.ProcessTransactionAsync(transaction);
        return Ok(processedTransaction);
    }
} 