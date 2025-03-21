using Rapidpay.Data.Models;

namespace Rapidpay.Data.Interfaces;

public interface IUnitOfWork
{
    IRepository<int, User> UserRepository{get;}
    IRepository<int, Card> CardRepository{get;}
    IRepository<int, Transaction> TransactionRepository{get;}
    Task SaveAsync();
}