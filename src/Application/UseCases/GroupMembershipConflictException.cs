using System;

namespace Application.UseCases
{
    /// <summary>
    /// Raised when a write-time check detects that group membership or member history
    /// no longer permits an operation that passed the use case's initial validation.
    /// </summary>
    public class GroupMembershipConflictException : Exception
    {
        public GroupMembershipConflictException(string message) : base(message)
        {
        }
    }
}
