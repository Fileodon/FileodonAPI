using System.Threading.Channels;
using Domain.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Test;

public sealed class EmailWorkerTests
{
    [Fact]
    public async Task Worker_sends_queued_email()
    {
        Channel<EmailWorkItem> channel = Channel.CreateUnbounded<EmailWorkItem>();
        Mock<IEmailTransport> transport = new();
        TaskCompletionSource<bool> sent = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport
            .Setup(item => item.SendAsync(It.IsAny<EmailWorkItem>(), It.IsAny<CancellationToken>()))
            .Callback(() => sent.TrySetResult(true))
            .Returns(Task.CompletedTask);
        EmailWorkItem workItem = new("user@example.com", "Subject", "<p>Body</p>");
        EmailWorker worker = new(channel, transport.Object, NullLogger<EmailWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await channel.Writer.WriteAsync(workItem);
        await sent.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await worker.StopAsync(CancellationToken.None);

        transport.Verify(
            item => item.SendAsync(workItem, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Worker_continues_after_a_send_failure()
    {
        Channel<EmailWorkItem> channel = Channel.CreateUnbounded<EmailWorkItem>();
        Mock<IEmailTransport> transport = new();
        TaskCompletionSource<bool> secondSent = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int sendCount = 0;
        transport
            .Setup(item => item.SendAsync(It.IsAny<EmailWorkItem>(), It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref sendCount) == 1)
                {
                    throw new InvalidOperationException("SMTP failure");
                }

                secondSent.TrySetResult(true);
            })
            .Returns(Task.CompletedTask);
        EmailWorker worker = new(channel, transport.Object, NullLogger<EmailWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await channel.Writer.WriteAsync(new EmailWorkItem("first@example.com", "First", "body"));
        await channel.Writer.WriteAsync(new EmailWorkItem("second@example.com", "Second", "body"));
        await secondSent.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await worker.StopAsync(CancellationToken.None);

        Assert.Equal(2, sendCount);
    }
}
