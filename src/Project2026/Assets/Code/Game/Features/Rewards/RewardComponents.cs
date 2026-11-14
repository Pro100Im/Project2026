using Code.Game.StaticData.Configs;
using Entitas;

namespace Code.Game.Features.Rewards
{
    [Game] [Meta] public struct Reward : IComponent { public EntityConfig Value; }
    [Game] public class TownGoldReward : IComponent { public int Value; }
    [Game] public class BattleTownGold : IComponent { public int Value; }
    [Game] public class BattleTownRewardsApplied : IComponent { }
}
