using System;

namespace Core.Entities
{
   public class Group
   {
      public int Id { get; set; }
      public int OwnerId { get; set; }

      public string Name { get; set; }

      public Guid UniqueKey { get; set; }

      public List<User> Members { get; set; }

      public Group()
      {
         Name = string.Empty;
         Members = new List<User>();
      }
   }
}