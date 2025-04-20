using System;

namespace Core.Entities
{
   public class Password
   {
      public int UserId { get; set; }

      public string Cipher { get; set; }
      public string Encrypted { get; set; }

      public Password()
      {
         Cipher = string.Empty;
         Encrypted = string.Empty;
      }
   }
}