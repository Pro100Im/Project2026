using Code.Infrastructure.View;
using Entitas;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayerCharacterLinkSystem : IExecuteSystem
{
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(32);

    public PlayerCharacterLinkSystem(GameContext game)
    {
        _entities = game.GetGroup(GameMatcher
         .AllOf(
            GameMatcher.Player,
            GameMatcher.ObjectId
            )
         .NoneOf(GameMatcher.View));
    }

    public void Execute()
    {
        foreach (GameEntity entity in _entities.GetEntities(_buffer))
        {
            var netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[entity.objectId.Value]; 
            var view = netObj.GetComponent<EntityBehaviour>(); 

            view.SetEntity(entity);
        }
    }
}
