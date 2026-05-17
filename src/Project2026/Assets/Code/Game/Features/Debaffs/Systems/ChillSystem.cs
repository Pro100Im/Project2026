using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Debaffs.Systems
{
    public class ChillSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _debaffs;

        private readonly List<GameEntity> _debaffsBuffer = new(124);

        public ChillSystem(GameContext gameContext)
        {
            _debaffs = gameContext.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.TotalDamage,
                    GameMatcher.OwnerId,
                    GameMatcher.TargetId,
                    GameMatcher.DamageRequest));
        }

        public void Execute()
        {
            var debuffs = _debaffs.GetEntities(_debaffsBuffer);

            for (var i = 0; i < debuffs.Count; i++)
            {
                var debuff = debuffs[i];
                

            }
        }
    }
}