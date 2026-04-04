using UnityEngine;

namespace Code.Common.Entity
{
    public static class GetGameEntityById
    {
        public static GameEntity Get(int value) =>
            Contexts.sharedInstance.game.GetEntityWithId(value);
    }
}