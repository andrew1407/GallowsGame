using System;
using System.Threading.Tasks;

// Blocks on async code from a test. The work runs on the thread pool, because awaits started on the editor's
// main thread continue on Unity's synchronization context, which a blocked main thread would never pump
public static class Sync
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    public static void Run(Func<Task> action)
    {
        Task task = Task.Run(action);
        if (!task.Wait(_timeout)) throw new TimeoutException($"Task wasn't completed in {_timeout.TotalSeconds} seconds");
    }

    public static T Run<T>(Func<Task<T>> action)
    {
        Task<T> task = Task.Run(action);
        if (!task.Wait(_timeout)) throw new TimeoutException($"Task wasn't completed in {_timeout.TotalSeconds} seconds");
        return task.Result;
    }
}
