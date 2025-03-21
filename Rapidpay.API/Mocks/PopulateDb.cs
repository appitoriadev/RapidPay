using Rapidpay.Business.Interfaces;

namespace Rapidpay.API.Mocks;

public static class PopulateDb
{
    public static async Task FillDb(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var cardService = scope.ServiceProvider.GetRequiredService<ICardService>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

            await authService.Register(new Data.Models.User
            {
                Username = "John Doe",
                Email = "john.doe@example.com",
                PasswordHash = "password123",
            });

            await authService.Register(new Data.Models.User
            {
                Username = "Jane Doe",
                Email = "jane.doe@example.com",
                PasswordHash = "password345",
            });


            var user = authService.GetUserByUsername("John Doe").Result!;
            var user2 = authService.GetUserByUsername("Jane Doe").Result!;
            await cardService.CreateCardAsync(new Data.Models.Card
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = "04",
                ExpiryYear = "2045",
                Cvv = "123",
                UserId = user.Id,
                CardHolderName = user.Username,
                CardType = Data.Models.CardType.Debit,
                CardBrand = Data.Models.CardBrand.Visa,
                Balance = 1320,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
            });

            await cardService.CreateCardAsync(new Data.Models.Card
            {
                CardNumber = "1234567890120000",
                ExpiryMonth = "03",
                ExpiryYear = "2035",
                Cvv = "123",
                UserId = user2.Id,
                CardHolderName = user2.Username,
                CardType = Data.Models.CardType.Credit,
                CardBrand = Data.Models.CardBrand.Mastercard,
                Balance = 2500,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
            });

            await transactionService.CreateTransactionAsync(new Data.Models.Transaction
            {
                Amount = 100,
                Currency = "USD",
                Status = Data.Models.TransactionStatus.Completed,
            });

            await transactionService.CreateTransactionAsync(new Data.Models.Transaction
            {
                Amount = 75,
                Currency = "USD",
                Status = Data.Models.TransactionStatus.Completed,
            });
        }
    }
}

