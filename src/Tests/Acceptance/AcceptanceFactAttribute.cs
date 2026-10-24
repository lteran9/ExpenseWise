namespace Tests.Acceptance
{
    public sealed class AcceptanceFactAttribute : FactAttribute
    {
        public AcceptanceFactAttribute()
        {
            if (!AcceptanceSettings.IsConfigured)
            {
                Skip = $"Set {AcceptanceSettings.UiUrlVariable} and {AcceptanceSettings.ApiUrlVariable} to run Docker acceptance tests.";
            }
        }
    }
}
