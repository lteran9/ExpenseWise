using System;

namespace Application.UseCases.Ports
{
    /// <summary>
    /// Raised when group membership changed after a request was validated and the write is no longer allowed.
    /// </summary>
    public class GroupMembershipConflictException : Exception
    {
        public GroupMembershipConflictException(string message) : base(message)
        {
        }
    }
}
