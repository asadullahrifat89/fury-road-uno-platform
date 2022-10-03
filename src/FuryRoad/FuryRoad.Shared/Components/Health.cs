namespace FuryRoad
{
    public class Health : GameObject
    {
        public Health()
        {
            Tag = Constants.HEALTH_TAG;
            SetContent(AssetTemplates.HEALTH_TEMPLATE);
        }
    }
}

