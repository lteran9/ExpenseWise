using System;

namespace Core.Entities
{
   public class User
   {
      public int Id { get; set; }

      public string Name { get; set; }
      public string Email { get; set; }
      public string Phone { get; set; }

      public Guid UniqueKey { get; set; }

      public List<Group> Groups { get; set; }

      public User()
      {
         Name = string.Empty;
         Email = string.Empty;
         Phone = string.Empty;

         Groups = new List<Group>();
      }
   }
}