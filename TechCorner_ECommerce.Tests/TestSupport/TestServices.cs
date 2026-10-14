using TechCorner_ECommerce.Services;

namespace TechCorner_ECommerce.Tests.TestSupport;

public class FixedUniqueCodeService : IUniqueCodeService {
    private int orderNumber;
    private int skuNumber;
    private int parentNumber;

    public string CreateParentProductPublicId() {
        parentNumber++;
        return $"PP-{parentNumber:000}";
    }

    public string CreateSkuCode() {
        skuNumber++;
        return $"SKU-{skuNumber:000}";
    }

    public string CreateOrderCode() {
        orderNumber++;
        return $"ORD-{orderNumber:000}";
    }
}

public class CapturingBackgroundTaskQueue : IBackgroundTaskQueue {
    public List<Func<IServiceProvider, CancellationToken, Task>> WorkItems { get; } = new();

    public void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem) {
        WorkItems.Add(workItem);
    }

    public ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) {
        throw new NotSupportedException("Queue draining is not needed for service unit tests.");
    }
}
