using Rapidpay.Data;
using Rapidpay.Data.Interfaces;
using Rapidpay.Data.Models;
using Rapidpay.Data.Repository;
using Microsoft.EntityFrameworkCore;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly RapidpayDbContext _context;
    private bool _disposed = false;
    private IRepository<int, User>? _userRepository;
    private IRepository<int, Card>? _cardRepository;
    private IRepository<int, Transaction>? _transactionRepository;
    private IRepository<int, AuthResponse>? _authRepository;
    public UnitOfWork(RapidpayDbContext context)
    {
        _context = context;
    }
    public IRepository<int, User> UserRepository
    {
        get
        {
            if (_userRepository == null)
            {
                _userRepository = new Repository<int, User>(_context);
            }
            return _userRepository;
        }
    }

    public IRepository<int, Card> CardRepository
    {
        get
        {
            if (_cardRepository == null)
            {
                _cardRepository = new Repository<int, Card>(_context);
            }
            return _cardRepository;
        }
    }

    public IRepository<int, Transaction> TransactionRepository
    {
        get
        {
            if (_transactionRepository == null)
            {
                _transactionRepository = new Repository<int, Transaction>(_context);
            }
            return _transactionRepository;
        }
    }

    public IRepository<int, AuthResponse> AuthRepository
    {
        get
        {
            if (_authRepository == null)
            {
                _authRepository = new Repository<int, AuthResponse>(_context);
            }
            return _authRepository;
        }
    }


    public async Task SaveAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ex.Entries.Single().Reload();
        }
    }
    
    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.DisposeAsync();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
    }
    #endregion
}
