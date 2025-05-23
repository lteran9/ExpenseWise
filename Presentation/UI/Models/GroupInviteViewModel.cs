namespace UI.Models
{
   public class GroupInviteViewModel
   {
      public string Phone { get; set; }

      public GroupViewModel Group { get; set; }

      public GroupInviteViewModel()
      {
         Phone = string.Empty;
         Group = new GroupViewModel();
      }
   }
}