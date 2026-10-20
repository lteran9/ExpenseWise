using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlDatabase
{
    /// <summary>
    /// Serializes balance-affecting writes within one group. Call it first inside the transaction, before any reads,
    /// so later reads observe the state committed before the lock was granted.
    /// </summary>
    internal static class GroupWriteLock
    {
        public static async Task AcquireAsync(CoreContext context, int groupId, CancellationToken cancellationToken)
        {
            if (!context.Database.IsRelational())
            {
                return;
            }

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT id FROM `groups` WHERE id = {groupId} FOR UPDATE",
                cancellationToken);
        }
    }
}
