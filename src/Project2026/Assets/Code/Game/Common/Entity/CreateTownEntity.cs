namespace Code.Game.Common.Entity
{
    public static class CreateTownEntity
    {
        public static TownEntity Empty() =>
          Contexts.sharedInstance.town.CreateEntity();
    }
}
