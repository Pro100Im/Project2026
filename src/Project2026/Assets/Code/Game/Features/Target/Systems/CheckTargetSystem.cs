using Entitas;

namespace Code.Game.Features.Target.Systems
{
    // To do
    public class CheckTargetSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _targets;
        private readonly IGroup<GameEntity> _enemies;

        public CheckTargetSystem(GameContext gameContext)
        {
            _enemies = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.Enemy));

            _targets = gameContext.GetGroup(GameMatcher
                .AllOf(
                GameMatcher.Targetable,
                GameMatcher.Player));
        }

        public void Execute()
        {
            //foreach (var enemy in _enemies)
            //{
            //    var minDist = float.MaxValue;
            //    var nearestTargetId = -1;

            //    foreach (var target in _targets)
            //    {
            //        if (target.isDead)
            //            continue;

            //        var dist = Vector3.Distance(enemy.transform.Value.position, target.transform.Value.position);

            //        if (dist < minDist)
            //        {
            //            minDist = dist;
            //            nearestTargetId = target.id.Value;
            //        }
            //    }

            //    if (nearestTargetId != -1)
            //    {
            //        if (!enemy.hasTargetId)
            //            enemy.AddTargetId(nearestTargetId);
            //        else if(enemy.targetId.Value != nearestTargetId)
            //            enemy.ReplaceTargetId(nearestTargetId);
            //    }
            //    else
            //    {
            //        if (enemy.hasTargetId)
            //            enemy.RemoveTargetId();
            //    }
            //}
        }
    }
}