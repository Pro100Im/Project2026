using Code.Infrastructure.Persistence;
using Entitas;
using System.Collections.Generic;

namespace Code.Game.Features.Town.Systems
{
    public class TownProgressLoadSystem : IInitializeSystem, IExecuteSystem
    {
        private readonly IUserProgressService _progress;
        private readonly IGroup<TownEntity> _exchequer;
        private readonly IGroup<TownEntity> _buildings;
        private readonly List<TownEntity> _buildingsBuffer = new(16);

        private bool _applied;

        public TownProgressLoadSystem(IUserProgressService progress)
        {
            _progress = progress;

            var town = Contexts.sharedInstance.town;
            _exchequer = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownExchequer,
                    TownMatcher.TownResources));
            _buildings = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingType,
                    TownMatcher.TownBuildingLevel,
                    TownMatcher.TownBuildingMaxLevel));
        }

        public void Initialize() => TryApply();

        public void Execute()
        {
            if (_applied)
                return;

            TryApply();
        }

        private void TryApply()
        {
            var exchequer = _exchequer.GetSingleEntity();

            if (exchequer == null || !HasResources(exchequer))
                return;

            var buildings = _buildings.GetEntities(_buildingsBuffer);

            if (buildings.Count == 0)
                return;

            ApplyResources(exchequer);
            ApplyBuildings(buildings);
            _applied = true;
        }

        private void ApplyResources(TownEntity exchequer)
        {
            var gold = ClampResource(_progress.Gold);
            var stone = ClampResource(_progress.Stone);
            var crystal = ClampResource(_progress.Crystal);
            var wood = ClampResource(_progress.Wood);
            var knowledge = ClampResource(_progress.Knowledge);

            if (gold != _progress.Gold ||
                stone != _progress.Stone ||
                crystal != _progress.Crystal ||
                wood != _progress.Wood ||
                knowledge != _progress.Knowledge)
                _progress.SetTownResources(gold, stone, crystal, wood, knowledge);

            exchequer.ReplaceTownGold(gold);
            exchequer.ReplaceTownStone(stone);
            exchequer.ReplaceTownCrystal(crystal);
            exchequer.ReplaceTownWood(wood);
            exchequer.ReplaceTownKnowledge(knowledge);

            var view = exchequer.townExchequer.Value;

            if (view == null)
                return;

            view.SetGold(gold, false);
            view.SetStone(stone, false);
            view.SetCrystal(crystal, false);
            view.SetWood(wood, false);
            view.SetKnowledge(knowledge, false);
        }

        private void ApplyBuildings(List<TownEntity> buildings)
        {
            for (var i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                var type = building.townBuildingType.Value;
                var hasSavedLevel = _progress.TryGetBuildingLevel(type, out var savedLevel);
                var level = hasSavedLevel ? savedLevel : 0;

                if (level < 0)
                    level = 0;

                var maxLevel = building.townBuildingMaxLevel.Value;

                if (level > maxLevel)
                    level = maxLevel;

                if (hasSavedLevel && savedLevel != level)
                    _progress.SetBuildingLevel(type, level);

                if (building.townBuildingLevel.Value != level)
                    building.ReplaceTownBuildingLevel(level);

                if (level > 0 && building.hasTownView)
                    building.townView.Value.SetViews(level - 1);
            }
        }

        private static bool HasResources(TownEntity exchequer) =>
            exchequer.hasTownGold &&
            exchequer.hasTownStone &&
            exchequer.hasTownCrystal &&
            exchequer.hasTownWood &&
            exchequer.hasTownKnowledge;

        private static int ClampResource(int value) => value < 0 ? 0 : value;
    }
}
