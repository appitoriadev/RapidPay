using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Rapidpay.Business.Interfaces;
using Rapidpay.Business.Services;
using Rapidpay.Data.Interfaces;
using Rapidpay.Data.Models;
using Xunit;

namespace Rapidpay.Tests.TransactionTests;

public class TransactionServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICardService> _cardServiceMock;
    private readonly Mock<IRepository<int, Transaction>> _transactionRepositoryMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cardServiceMock = new Mock<ICardService>();
        _transactionRepositoryMock = new Mock<IRepository<int, Transaction>>();

        _unitOfWorkMock.Setup(uow => uow.TransactionRepository)
            .Returns(_transactionRepositoryMock.Object);

        _transactionService = new TransactionService(_unitOfWorkMock.Object, _cardServiceMock.Object);
    }

    public void Dispose()
    {
        // Reset mocks after each test
        _unitOfWorkMock.Reset();
        _cardServiceMock.Reset();
        _transactionRepositoryMock.Reset();
    }

    [Fact]
    public async Task CreateTransactionAsync_ShouldCreateAndReturnTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            CardId = 1,
            Amount = 100.00m,
            Currency = "USD",
            Status = TransactionStatus.Pending,
            Description = "Test transaction"
        };

        _transactionRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);
        
        _unitOfWorkMock.Setup(uow => uow.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateTransactionAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TransactionStatus.Pending, result.Status);
        _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_ShouldReturnTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = 1,
            CardId = 1,
            Amount = 100.00m,
            Currency = "USD",
            Status = TransactionStatus.Completed
        };

        _transactionRepositoryMock.Setup(repo => repo.FindAsync(1))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.GetTransactionByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(100.00m, result.Amount);
        _transactionRepositoryMock.Verify(repo => repo.FindAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByCardIdAsync_ShouldReturnTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new() { 
                Id = 1, 
                CardId = 1, 
                Amount = 100.00m, 
                Currency = "USD",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddHours(-1) 
            },
            new() { 
                Id = 2, 
                CardId = 1, 
                Amount = 200.00m, 
                Currency = "USD",
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow 
            }
        };

        _transactionRepositoryMock.Setup(repo => repo.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>(),
                It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
                It.IsAny<string>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Transaction, bool>> filter,
                         Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>? orderBy,
                         string includeProperties) =>
            {
                var query = transactions.AsQueryable();
                if (filter != null)
                {
                    query = query.Where(filter);
                }
                if (orderBy != null)
                {
                    query = orderBy(query);
                }
                return query.ToList();
            });

        // Act
        var result = await _transactionService.GetTransactionsByCardIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(200.00m, result.First().Amount); // Most recent transaction should be first
        _transactionRepositoryMock.Verify(repo => repo.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>(),
            It.IsAny<Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>>>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ProcessTransactionAsync_WithValidCard_ShouldCompleteTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = 1,
            CardId = 1,
            Amount = 100.00m,
            Currency = "USD",
            Status = TransactionStatus.Pending
        };

        var card = new Card { 
            Id = 1, 
            Balance = 500.00m,
            CardNumber = "1234567890123456",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            Cvv = "123",
            CardHolderName = "Test User"
        };

        _cardServiceMock.Setup(service => service.GetCardByIdAsync(1))
            .ReturnsAsync(card);

        _unitOfWorkMock.Setup(uow => uow.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.ProcessTransactionAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TransactionStatus.Completed, result.Status);
        Assert.NotNull(result.CompletedAt);
        _cardServiceMock.Verify(service => service.GetCardByIdAsync(1), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessTransactionAsync_WithInvalidCard_ShouldThrowException()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = 1,
            CardId = 999, // Non-existent card
            Amount = 100.00m,
            Currency = "USD",
            Status = TransactionStatus.Pending
        };

        _cardServiceMock.Setup(service => service.GetCardByIdAsync(999))
            .ReturnsAsync((Card?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _transactionService.ProcessTransactionAsync(transaction));
        
        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Never);
    }
} 