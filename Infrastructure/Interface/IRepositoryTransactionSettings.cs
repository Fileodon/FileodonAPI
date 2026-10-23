namespace Infrastructure.Interface;

public interface IRepositoryTransactionSettings
{
    bool AutomaticTransactionsEnabled { get; }
    IDisposable DisableAutomaticTransactions();
}
