using Code.Infrastructure.View;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

namespace Code.Game.Features.FloatingText
{
    /// <summary>
    /// Text that floats up and fades out. One persistent tween per (pooled) view, restarted on every show;
    /// damage numbers are written with SetText, so showing one allocates nothing.
    /// </summary>
    public class FloatingTextView : EntityDependant
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _floatDistance = 0.6f;
        [SerializeField] private float _duration = 0.8f;

        private Tween _tween;
        private DG.Tweening.Core.DOGetter<float> _getProgress;
        private DG.Tweening.Core.DOSetter<float> _setProgress;
        private TweenCallback _onTweenComplete;

        private float _progress;
        private Vector3 _startPosition;
        private Color _color;
        private Action _onComplete;

        private void Awake()
        {
            EnsureTween();
        }

        private void OnEnable()
        {
            if (Entity == null || !Entity.hasFloatingText || _text == null)
                return;

            _text.SetText("{0}", Entity.floatingText.Value);

            Begin(Entity.floatingTextColor.Value, null);
        }

        public void Play(string text, Color color = default, Action onComplete = null)
        {
            if (_text == null)
                return;

            _text.text = text;

            Begin(color, onComplete);
        }

        private void Begin(Color color, Action onComplete)
        {
            if (color == default)
                color = Color.white;

            EnsureTween();

            _color = color;
            _text.color = color;
            _startPosition = transform.position;
            _onComplete = onComplete;

            _tween.Restart();
        }

        private void OnDisable()
        {
            _onComplete = null;

            if (_tween != null && _tween.active)
                _tween.Pause();
        }

        private void OnDestroy()
        {
            if (_tween != null && _tween.active)
                _tween.Kill();

            _tween = null;
        }

        private void EnsureTween()
        {
            if (_tween != null && _tween.active)
                return;

            _getProgress ??= GetProgress;
            _setProgress ??= SetProgress;
            _onTweenComplete ??= OnTweenComplete;

            _tween = DOTween.To(_getProgress, _setProgress, 1f, _duration)
                .From(0f, setImmediately: false)
                .SetEase(Ease.Linear)
                .SetAutoKill(false)
                .OnComplete(_onTweenComplete)
                .Pause();
        }

        private float GetProgress() => _progress;

        private void SetProgress(float progress)
        {
            _progress = progress;

            if (_text == null)
                return;

            var rise = DOVirtual.EasedValue(0f, _floatDistance, progress, Ease.OutQuad);
            transform.position = _startPosition + (Vector3.up * rise);

            var color = _color;
            color.a = DOVirtual.EasedValue(_color.a, 0f, progress, Ease.InQuad);
            _text.color = color;
        }

        private void OnTweenComplete()
        {
            var onComplete = _onComplete;
            _onComplete = null;
            onComplete?.Invoke();
        }
    }
}
