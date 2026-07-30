using Code.Game.Features.Target.Systems;
using Code.Infrastructure.Systems;

namespace Code.Game.Features.Target
{
    public class TargetFeature : Feature
    {
        public TargetFeature(ISystemFactory systemFactory)
        {
            // 1. Cleanup
            Add(systemFactory.Create<ReleaseSurroundSlotSystem>());

            // 2. Combat brain: TargetId → slot
            Add(systemFactory.Create<SelectCombatTargetSystem>());
            Add(systemFactory.Create<AssignSurroundSlotSystem>());
            Add(systemFactory.Create<RepositionRangedSurroundSlotSystem>());

            // 3. Defense AI
            Add(systemFactory.Create<UpdateCastleThreatSystem>());
            Add(systemFactory.Create<AssignRallyToCastleSystem>());
            Add(systemFactory.Create<AssignRallySurroundSlotSystem>());
            Add(systemFactory.Create<DefensePatrolSystem>());

            // 4. Path step
            Add(systemFactory.Create<RequestTargetCellSystem>());
            Add(systemFactory.Create<SelectTargetCellSystem>());
        }
    }
}
