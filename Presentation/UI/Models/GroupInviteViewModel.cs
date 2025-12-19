namespace UI.Models
{
    public class GroupInviteViewModel
    {
        public string Phone { get; set; }
        public string CountryCode { get; set; }

        public GroupViewModel Group { get; set; }

        public GroupInviteViewModel()
        {
            Phone = string.Empty;
            CountryCode = string.Empty;
            Group = new GroupViewModel();
        }
    }
}
