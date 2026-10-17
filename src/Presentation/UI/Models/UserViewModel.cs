using System;

namespace UI.Models
{
    public class UserViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CountryCode { get; set; }

        public Guid Id { get; set; }

        public UserViewModel()
        {
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            CountryCode = string.Empty;
        }
    }
}
