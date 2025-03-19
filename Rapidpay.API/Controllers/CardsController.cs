using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rapidpay.Business.Interfaces;
using Rapidpay.Data.Models;
using System.Security.Claims;

namespace Rapidpay.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService)
    {
        _cardService = cardService;
    }

    [HttpPost]
    public async Task<ActionResult<Card>> CreateCard(Card card)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        card.UserId = userId;
        
        var createdCard = await _cardService.CreateCardAsync(card);
        return CreatedAtAction(nameof(GetCard), new { id = createdCard.Id }, createdCard);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Card>> GetCard(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var card = await _cardService.GetCardByIdAsync(id);
        
        if (card == null)
        {
            return NotFound();
        }

        if (card.UserId != userId)
        {
            return Forbid();
        }

        return Ok(card);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Card>>> GetUserCards()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var cards = await _cardService.GetCardsByUserIdAsync(userId);
        return Ok(cards);
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateCard([FromBody] CardValidationRequest request)
    {
        var isValid = await _cardService.ValidateCardAsync(
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Cvv);

        return Ok(isValid);
    }
}

public class CardValidationRequest
{
    public string CardNumber { get; set; }
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
    public string Cvv { get; set; }
} 