namespace TechCorner_ECommerce.Services {
    public interface IBackgroundTaskQueue {
        void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem);
        ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
