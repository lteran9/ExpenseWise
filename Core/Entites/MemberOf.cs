using System;

namespace Core.Entities
{
   public class MemberOf
   {
      public User User { get; set; }
      public Group Group { get; set; }

      public MemberOf()
      {
         User = new User();
         Group = new Group();
      }
   }
}