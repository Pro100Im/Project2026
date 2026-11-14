using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.GameSession.Systems
{
    public class ApplyBattleTownRewardsSystem : IExecuteSystem
    {
        private readonly IUserProgressService _progress;
        private readonly IGroup<GameEntity> _sessions;
        private readonly IGroup<TownEntity> _townExchequers;

        private readonly List<GameEntity> _sessionBuffer = new(2);

        public ApplyBattleTownRewardsSystem(IUserProgressService progress)
        {
            _progress = progress;

            _sessions = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GameSession,
                    GameMatcher.ForcedPause,
                    GameMatcher.BattleTownGold)
                .NoneOf(GameMatcher.BattleTownRewardsApplied));

            _townExchequers = Contexts.sharedInstance.town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownExchequer,
                    TownMatcher.TownGold));
        }

        public void Execute()
        {
            var sessions = _sessions.GetEntities(_sessionBuffer);

            for (var i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                var battleGold = session.battleTownGold.Value;

                if (battleGold > 0)
                {
                    var gold = _progress.Gold + battleGold;

                    _progress.SetTownResources(
                        gold,
                        _progress.Stone,
                        _progress.Crystal,
                        _progress.Wood,
                        _progress.Knowledge);
                    _progress.Flush();

                    SyncTownGold(gold);
                }

                session.isBattleTownRewardsApplied = true;
            }
        }

        private void SyncTownGold(int gold)
        {
            if (_townExchequers.count != 1)
                return;

            var exchequer = _townExchequers.GetSingleEntity();
            exchequer.ReplaceTownGold(gold);
            exchequer.townExchequer.Value.SetGold(gold);
        }
    }
}
