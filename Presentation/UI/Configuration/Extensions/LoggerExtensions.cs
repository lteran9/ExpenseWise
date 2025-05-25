using System;

namespace UI.Configuration
{
   public static class LoggerExtensions
   {
      public static void LogError(this ILogger logger, Exception ex)
      {
         logger.LogError(ex.InnerException?.Message ?? ex.Message);
      }
   }
}