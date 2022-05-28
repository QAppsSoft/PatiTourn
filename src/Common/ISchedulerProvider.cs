using System.Reactive.Concurrency;

namespace Common
{
    public interface ISchedulerProvider
    {
        IScheduler CurrentThread { get; }
        IScheduler Dispatcher { get; }
        IScheduler Immediate { get; }
        IScheduler NewThread { get; }
        IScheduler TaskPoolLongRunning { get; }
        IScheduler TaskPool { get; }
    }
}
