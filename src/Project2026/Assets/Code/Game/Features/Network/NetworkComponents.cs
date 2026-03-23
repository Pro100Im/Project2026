using Entitas;

namespace Code.Game.Features.Network
{
    [Network][Input][Game] public class ClientId : IComponent { public ulong Value; }
    [Network][Game] public class ObjectId : IComponent { public ulong Value; }
    [Network][Input] public class LocalPlayer : IComponent { }
  
    public enum RequestTypes
    {
        None,
        CreatePlayerEntity,
        ReceiveObjectId,
        ReceiveSpawnPosition
    }
}