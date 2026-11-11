using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Domain.Worker;

public sealed record class EmailWorkItem(
    string Recipient,
    string Subject,
    string HtmlBody
);

public interface IEmailQueue
{
    bool Enqueue(EmailWorkItem workItem);
}

public sealed class EmailQueue(Channel<EmailWorkItem> channel) : IEmailQueue
{
    public bool Enqueue(
        EmailWorkItem workItem
    )
    {
        return channel.Writer.TryWrite(workItem);
    }
}

public interface IEmailTransport
{
    Task SendAsync(EmailWorkItem workItem, CancellationToken cancellationToken);
}

public class EmailWorker : BackgroundService
{
    private readonly ChannelReader<EmailWorkItem> _channelReader;
    private readonly IEmailTransport _emailTransport;
    private readonly ILogger<EmailWorker> _logger;

    public EmailWorker(Channel<EmailWorkItem> channel, IEmailTransport emailTransport, ILogger<EmailWorker> logger)
    {
        _channelReader = channel.Reader;
        _emailTransport = emailTransport;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _channelReader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _emailTransport.SendAsync(workItem, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while sending email to {Recipient} with subject {Subject}",
                    workItem.Recipient,
                    workItem.Subject);
            }
        }
    }
}
