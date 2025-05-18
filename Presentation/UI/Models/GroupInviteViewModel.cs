namespace UI.Models
{
   public class GroupInviteViewModel
   {
      public string Name { get; set; }
      public string Phone { get; set; }

      public GroupViewModel Group { get; set; }

      public GroupInviteViewModel()
      {
         Name = string.Empty;
         Phone = string.Empty;
         Group = new GroupViewModel();
      }
   }
}