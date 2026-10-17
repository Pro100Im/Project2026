using Code.Infrastructure.View;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

namespace Code.Game.Features.FloatingText
{
    public class FloatingTextView : EntityDependant
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _floatDistance = 0.6f;
        [SerializeField] private float _duration = 0.8f;

        private Tween _moveTween;
        private Tween _fadeTween;

        private void OnEnable()
        {
            if (Entity == null || !Entity.hasFloatingText || _text == null)
                return;

            var text = Mathf.RoundToInt(Entity.floatingText.Value).ToString();

            Play(text, Entity.floatingTextColor.Value, null);
        }

        public void Play(string text, Color color = default, Action onComplete = null)
        {
            if (_text == null)
                return;

            if (color == default)
                color = Color.white;

            KillTweens();

            _text.text = text;
            _text.color = color;

            var endPos = transform.position + Vector3.up * _floatDistance;

            _moveTween = transform.DOMove(endPos, _duration).SetEase(Ease.OutQuad);
            _fadeTween = DOTween.ToAlpha(() => _text.color, c => _text.color = c, 0f, _duration)
                .SetEase(Ease.InQuad);

            _fadeTween.OnComplete(() =>
            {
                _moveTween = null;
                _fadeTween = null;

                onComplete?.Invoke();
            });
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
