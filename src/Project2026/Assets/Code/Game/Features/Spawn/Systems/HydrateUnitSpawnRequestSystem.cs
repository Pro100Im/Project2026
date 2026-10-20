using Assets.Code.Game.StaticData.Property;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Spawn.Systems
{
    public class HydrateUnitSpawnRequestSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _requests;
        private readonly IGroup<GameEntity> _slots;
        private readonly List<GameEntity> _requestsBuffer = new(8);
        private readonly List<GameEntity> _slotsBuffer = new(4);

        public HydrateUnitSpawnRequestSystem()
        {
            var game = Contexts.sharedInstance.game;
            _requests = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpawnRequsted,
                    GameMatcher.Player,
                    GameMatcher.SpawnSlot));
            _slots = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.UnitSlot,
                    GameMatcher.DefaultUnitConfig));
        }

        public void Execute()
        {
            var requests = _requests.GetEntities(_requestsBuffer);

            for (var i = 0; i < requests.Count; i++)
            {
                var request = requests[i];

                if (request.hasEntityConfig)
                    continue;

                var slot = FindSlot(request.spawnSlot.Value);

                if (slot == null)
                    continue;

                var config = slot.hasResolvedUnitConfig
                    ? slot.resolvedUnitConfig.Value
                    : slot.defaultUnitConfig.Value;

                if (config == null)
                    continue;

                request.AddEntityConfig(config);

                var size = config.GetProperty<UnitSizeProperty>();

                if (size == null || request.hasUnitSize)
                    continue;

                request.AddUnitSize(size.Size);
            }
        }

        private GameEntity FindSlot(int index)
        {
            var slots = _slots.GetEntities(_slotsBuffer);

            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].unitSlot.Value == index)
                    return slots[i];
            }

            return null;
        }
    }
}
