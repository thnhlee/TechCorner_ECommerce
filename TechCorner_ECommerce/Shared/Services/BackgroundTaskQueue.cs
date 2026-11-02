using System.Threading.Channels;

namespace TechCorner_ECommerce.Services {
    public class BackgroundTaskQueue : IBackgroundTaskQueue {
        private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> queue =
            Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>(
                new UnboundedChannelOptions {
                    SingleReader = true,
                    SingleWriter = false
                });

        public void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem) {
            ArgumentNullException.ThrowIfNull(workItem);

            if (!queue.Writer.TryWrite(workItem)) {
                throw new InvalidOperationException("Could not queue background work item.");
            }
        }

        public ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) {
            return queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
