using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace Tests.Regression
{
   public class AutoMoqAttribute : AutoDataAttribute
   {
      public AutoMoqAttribute() : base(() => new Fixture().Customize(new AutoMoqCustomization())) { }
   }
}