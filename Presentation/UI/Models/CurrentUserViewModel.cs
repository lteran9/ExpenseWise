namespace UI.Models
{
    /// <summary>
    /// Represents the currently authenticated user information available to all views.
    /// </summary>
    public class CurrentUserViewModel
    {
        /// <summary>
        /// The unique identifier of the authenticated user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Indicates whether a user is currently authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }
    }
}
