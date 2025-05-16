using System;

namespace UI.Models
{
   public class GroupViewModel
   {
      public string Name { get; set; }

      public Guid OwnerId { get; set; }
      public Guid UniqueKey { get; set; }

      public DateTime? StartDate { get; set; }
      public DateTime? EndDate { get; set; }

      public GroupViewModel()
      {
         Name = string.Empty;
      }
   }
}