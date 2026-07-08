using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Fodot.Bridge;

public static class TaskLog
{
    public static async Task LogBy(
        this Task task,
        Action<object> logger,
        CancellationToken ct = default,
        [CallerArgumentExpression("task")] string expr = "",
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            if (!ct.IsCancellationRequested) logger.Invoke($"Task failed with {ex}: {expr} at {member} ({Path.GetFileName(file)}:{line})");
            throw;
        }
        catch (Exception ex)
        {
            logger.Invoke($"Task failed with {ex}: {expr} at {member} ({Path.GetFileName(file)}:{line})");
            throw;
        }
    }
    
    public static async Task<T> LogBy<T>(
        this Task<T> task,
        Action<object> logger,
        CancellationToken ct = default,
        [CallerArgumentExpression("task")] string expr = "",
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return result;
        }
        catch (OperationCanceledException ex)
        {
            if (!ct.IsCancellationRequested) logger.Invoke($"Task failed with {ex}: {expr} at {member} ({Path.GetFileName(file)}:{line})");
            throw;
        }
        catch (Exception ex)
        {
            logger.Invoke($"Task failed with {ex}: {expr} at {member} ({Path.GetFileName(file)}:{line})");
            throw;
        }
    }
}