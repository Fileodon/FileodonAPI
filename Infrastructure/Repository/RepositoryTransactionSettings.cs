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

    private sealed class TransactionSettingsScope : IDisposable
    {
        private bool _disposed;
        private readonly RepositoryTransactionSettings _settings;

        public TransactionSettingsScope(RepositoryTransactionSettings settings)
        {
            _settings = settings;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _settings.EnableAutomaticTransactions();
        }
    }
}
