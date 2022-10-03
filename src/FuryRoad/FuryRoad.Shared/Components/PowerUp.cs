namespace FuryRoad
{
    public class PowerUp : GameObject
    {
        public PowerUp()
        {
            Tag = Constants.POWERUP_TAG;

            SetContent(AssetTemplates.POWERUP_TEMPLATE);
        }
    }
}

