using Code.Game.StaticData.Configs;
using Entitas;

namespace Code.Game.Features.Rewards
{
    [Game] [Meta] public struct Reward : IComponent { public EntityConfig Value; }
}