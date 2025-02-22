using System;

namespace Core.Entities
{
   public class Split
   {
      public int UserId { get; set; }
      public int GroupId { get; set; }
      public int ExpenseId { get; set; }

      public bool Paid { get; set; }

      public DateTime? PaidOn { get; set; }
   }
}