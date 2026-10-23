using Infrastructure.Interface;

namespace Infrastructure.Repository;

public sealed class RepositoryTransactionSettings : IRepositoryTransactionSettings
{
    private int _disabled;

    public bool AutomaticTransactionsEnabled => _disabled == 0;

    public IDisposable DisableAutomaticTransactions()
    {
        _disabled++;
        return new TransactionSettingsScope(this);
    }

    private void EnableAutomaticTransactions()
    {
        if (_disabled > 0)
        {
            _disabled--;
        }
    }

    private sealed class TransactionSettingsScope(RepositoryTransactionSettings settings) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            settings.EnableAutomaticTransactions();
        }
    }
}
