namespace Code.Common.Entity
{
    public static class CreateNetworkEntity
    {
        public static NetworkEntity Empty() =>
          Contexts.sharedInstance.network.CreateEntity();
    }
}