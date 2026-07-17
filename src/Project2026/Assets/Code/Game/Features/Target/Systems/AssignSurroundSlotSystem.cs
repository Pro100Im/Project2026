using Code.Game.Common.Entity;
using Code.Game.Features.Target.Services;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Target.Systems
{
    public class AssignSurroundSlotSystem : IExecuteSystem
    {
        private const int MaxTargetCandidates = 8;

        private readonly TargetService _targetService;

        private readonly IGroup<GameEntity> _units;
        private readonly IGroup<GameEntity> _maps;

        private readonly List<GameEntity> _buffer = new(256);
        private readonly HashSet<int> _processedTargets = new(256);
        private readonly List<TargetCandidate> _targetCandidates = new(MaxTargetCandidates);

        private struct TargetCandidate
        {
            public int TargetId;
            public GameEntity Target;
            public int MinX;
            public int MinY;
            public int MaxX;
            public int MaxY;
            public float SqrDist;
        }

        public AssignSurroundSlotSystem(GameContext context, TargetService targetService)
        {
            _targetService = targetService;

            _units = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.GridMovement,
                    GameMatcher.CurrentCell,
                    GameMatcher.Transform,
                    GameMatcher.Id,
                    GameMatcher.Team,
                    GameMatcher.Range,
                    GameMatcher.DetectionRange,
                    GameMatcher.UnitSize,
                    GameMatcher.MovementAvailable)
                .NoneOf(
                    GameMatcher.SurroundSlot,
                    GameMatcher.Dead));

            _maps = context.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.SpatialHash,
                    GameMatcher.TilemapMovement,
                    GameMatcher.SurroundField,
                    GameMatcher.OccupField,
                    GameMatcher.ReservedField));
        }

        public void Execute()
        {
            var mapEntity = _maps.GetSingleEntity();

            if (mapEntity == null)
                return;

            var spatialHash = mapEntity.spatialHash.Value;
            var tilemap = mapEntity.tilemapMovement.Value;
            var surroundField = mapEntity.surroundField.Value;
            var units = _units.GetEntities(_buffer);

            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var unitCell = unit.currentCell.Value;
                var unitId = unit.id.Value;
                var myTeam = unit.team.Value;
                var size = unit.unitSize.Value;
                var range = unit.range.Value;
                var detectionRange = unit.detectionRange.Value;
                var unitOriginPos = unit.woldPos.Value;

                if (unit.hasUnitAnchorPoint)
                    unitOriginPos += unit.unitAnchorPoint.Value;

                var physicalDetectionRange = TargetService.GetPhysicalRange(detectionRange);
                var sqrPhysicalDetectionRange = physicalDetectionRange * physicalDetectionRange;
                var iRange = Mathf.CeilToInt(TargetService.GetEffectiveRange(detectionRange));

                _targetCandidates.Clear();
                _processedTargets.Clear();

                for (var x = -iRange; x <= iRange; x++)
                {
                    for (var y = -iRange; y <= iRange; y++)
                    {
                        var checkPos = new Vector2Int(unitCell.x + x, unitCell.y + y);

                        if (!spatialHash.TryGetValue(checkPos, out var potentialTargets))
                            continue;

                        for (var j = 0; j < potentialTargets.Count; j++)
                        {
                            var targetId = potentialTargets[j];

                            if (targetId == unitId || !_processedTargets.Add(targetId))
                                continue;

                            var target = GetGameEntityById.Get(targetId);

                            if (target == null || target.team.Value == myTeam || !target.isTargetable || target.isDead)
                                continue;

                            if (!target.hasCurrentCell)
                                continue;

                            TargetService.GetFootprint(target, out var minX, out var minY, out var maxX, out var maxY);

                            var targetPoint = TargetService.GetClosestPoint(target, unitOriginPos);
                            var dx = unitOriginPos.x - targetPoint.x;
                            var dy = unitOriginPos.y - targetPoint.y;
                            var sDist = (dx * dx) + (dy * dy);

                            if (sDist > sqrPhysicalDetectionRange)
                                continue;

                            TryInsertCandidate(new TargetCandidate
                            {
                                TargetId = targetId,
                                Target = target,
                                MinX = minX,
                                MinY = minY,
                                MaxX = maxX,
                                MaxY = maxY,
                                SqrDist = sDist
                            });
                        }
                    }
                }

                for (var c = 0; c < _targetCandidates.Count; c++)
                {
                    var candidate = _targetCandidates[c];

                    if (!_targetService.TryPickSurroundSlot(
                            unitCell,
                            candidate.Target,
                            candidate.MinX,
                            candidate.MinY,
                            candidate.MaxX,
                            candidate.MaxY,
                            range,
                            size,
                            unitId,
                            mapEntity,
                            tilemap,
                            surroundField,
                            out var slot,
                            unit.isRangeAttack))
                        continue;

                    surroundField[slot] = unitId;
                    unit.AddSurroundSlot(slot);
                    unit.AddSurroundTargetId(candidate.TargetId);
                    break;
                }
            }
        }

        private void TryInsertCandidate(TargetCandidate candidate)
        {
            var insertIndex = _targetCandidates.Count;

            for (var i = 0; i < _targetCandidates.Count; i++)
            {
                if (candidate.SqrDist < _targetCandidates[i].SqrDist)
                {
                    insertIndex = i;
                    break;
                }
            }

            if (insertIndex >= MaxTargetCandidates)
                return;

            if (_targetCandidates.Count < MaxTargetCandidates)
                _targetCandidates.Insert(insertIndex, candidate);
            else
            {
                _targetCandidates.RemoveAt(MaxTargetCandidates - 1);
                _targetCandidates.Insert(insertIndex, candidate);
            }
        }
    }
}
