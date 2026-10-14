namespace TechCorner_ECommerce.Services {
    public class QueuedHostedService : BackgroundService {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<QueuedHostedService> logger;

        public QueuedHostedService(
            IBackgroundTaskQueue taskQueue,
            IServiceProvider serviceProvider,
            ILogger<QueuedHostedService> logger) {
            this.taskQueue = taskQueue;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                Func<IServiceProvider, CancellationToken, Task> workItem;

                try {
                    workItem = await taskQueue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                    break;
                }

                try {
                    using var scope = serviceProvider.CreateScope();
                    await workItem(scope.ServiceProvider, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                    break;
                }
                catch (Exception ex) {
                    logger.LogError(ex, "Background work item failed.");
                }
            }
        }
    }
}
