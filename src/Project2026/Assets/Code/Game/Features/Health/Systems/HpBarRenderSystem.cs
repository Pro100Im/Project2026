using Code.Game.Common.Entity;
using Code.Game.Common.Render;
using Code.Game.Features.Level;
using Code.Game.StaticData.Configs;
using Entitas;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Features.Health.Systems
{
    /// <summary>
    /// Draws every HP bar as 3 quads (frame, back fill, front fill) into one batched mesh: all bars are a single
    /// draw call, no GameObject per bar. Runs after InterpolateUnitViewSystem, so a bar sticks to the drawn
    /// (interpolated) unit position. The mesh is sized once per level from the level budget.
    /// </summary>
    public class HpBarRenderSystem : IExecuteSystem, ITearDownSystem
    {
        private const int QuadsPerBar = 3;

        private readonly BattleOverlayConfig _config;
        private readonly IGroup<GameEntity> _maps;
        private readonly IGroup<GameEntity> _hpBars;
        private readonly List<GameEntity> _buffer = new(LevelBuffers.DefaultCapacity);

        private BatchedQuadMesh _mesh;
        private Color32 _frontColor;
        private Color32 _backFillColor;
        private Color32 _backColor;

        public HpBarRenderSystem(BattleOverlayConfig config)
        {
            _config = config;

            var game = Contexts.sharedInstance.game;
            _maps = game.GetGroup(GameMatcher.Arena);
            _hpBars = game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.HpBar,
                    GameMatcher.HpBarFill,
                    GameMatcher.TargetId));
        }

        public void Execute()
        {
            var map = _maps.GetSingleEntity();

            if (map == null || _config == null || _config.HpBarMaterial == null)
                return;

            var arena = map.arena.Value;

            EnsureMesh(arena.MaxItems * QuadsPerBar);

            var hpBars = _hpBars.GetEntities(LevelBuffers.Fit(_buffer, arena));

            _mesh.Begin();

            for (var i = 0; i < hpBars.Count; i++)
            {
                var bar = hpBars[i];

                if (bar.isDestructed || !TryGetPosition(bar, out var position))
                    continue;

                var shape = bar.hpBar.Value;
                var fill = bar.hpBarFill.Value;

                var halfWidth = shape.Width * 0.5f;
                var halfHeight = shape.Height * 0.5f;
                var left = position.x - halfWidth;
                var bottom = position.y - halfHeight;
                var top = position.y + halfHeight;

                _mesh.TryAdd(
                    left - shape.Border, bottom - shape.Border,
                    position.x + halfWidth + shape.Border, top + shape.Border,
                    _backColor, Vector4.zero);

                if (fill.BackFill > 0f)
                    _mesh.TryAdd(left, bottom, left + (shape.Width * Mathf.Clamp01(fill.BackFill)), top, _backFillColor, Vector4.zero);

                if (fill.Fill > 0f)
                    _mesh.TryAdd(left, bottom, left + (shape.Width * Mathf.Clamp01(fill.Fill)), top, _frontColor, Vector4.zero);
            }

            _mesh.End();
        }

        public void TearDown()
        {
            _mesh?.Dispose();
            _mesh = null;
        }

        private void EnsureMesh(int quads)
        {
            if (_mesh == null)
            {
                _frontColor = _config.HpBarFrontColor;
                _backFillColor = _config.HpBarBackFillColor;
                _backColor = _config.HpBarBackColor;
                _mesh = new BatchedQuadMesh("HpBars", _config.HpBarMaterial, _config.HpBarSortingOrder, quads);
                return;
            }

            _mesh.EnsureCapacity(quads);
        }

        /// <summary>Same placement rules as AttachPosToTargetSystem used for the old bar GameObjects.</summary>
        private static bool TryGetPosition(GameEntity bar, out Vector3 position)
        {
            var target = GetGameEntityById.Get(bar.targetId.Value);

            if (target == null)
            {
                position = default;
                return false;
            }

            var offset = bar.hasMovementOffset ? bar.movementOffset.Value : Vector3.zero;

            if (target.hasSimSlot && target.hasTransform)
                position = target.transform.Value.position + offset;
            else if (target.isMoving && target.hasWoldPos)
                position = target.woldPos.Value + offset;
            else if (bar.hasSpawnPosition)
                position = bar.spawnPosition.Value;
            else
                position = (target.hasWoldPos ? target.woldPos.Value : Vector3.zero) + offset;

            return true;
        }
    }
}
