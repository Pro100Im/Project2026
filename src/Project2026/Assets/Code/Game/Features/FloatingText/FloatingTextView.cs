using Code.Infrastructure.View;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.Game.Features.FloatingText
{
    public class FloatingTextView : EntityDependant
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private Color _damageColor = new(1f, 0.35f, 0.25f, 1f);
        [SerializeField] private Color _healColor = new(0.35f, 1f, 0.45f, 1f);
        [SerializeField] private float _floatDistance = 0.6f;
        [SerializeField] private float _duration = 0.8f;

        private Tween _moveTween;
        private Tween _fadeTween;

        private void OnEnable()
        {
            if (Entity == null || !Entity.hasFloatingText || _text == null)
                return;

            KillTweens();

            var color = Entity.isHealFloatingText ? _healColor : _damageColor;
            _text.text = Mathf.RoundToInt(Entity.floatingText.Value).ToString();
            _text.color = color;

            var startPos = transform.position;
            var endPos = startPos + Vector3.up * _floatDistance;

            _moveTween = transform.DOMove(endPos, _duration).SetEase(Ease.OutQuad);
            _fadeTween = DOTween.ToAlpha(() => _text.color, c => _text.color = c, 0f, _duration)
                .SetEase(Ease.InQuad);
        }

        private void OnDisable()
        {
            KillTweens();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void KillTweens()
        {
            _moveTween?.Kill();
            _fadeTween?.Kill();
            _moveTween = null;
            _fadeTween = null;
        }
    }
}
