using Code.Game.Common.Entity;
using DG.Tweening;
using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    /// <summary>
    /// Range circle of the selected unit. Show and hide are two persistent tweens over elapsed seconds,
    /// created once and restarted, so selecting units allocates nothing.
    /// </summary>
    public class UnitRangeView : MonoBehaviour
    {
        private const float ShowFadeDuration = 0.25f;
        private const float ShowScaleDuration = 0.3f;
        private const float HideDuration = 0.2f;

        [SerializeField] private SpriteRenderer _rangeView;
        [SerializeField] private float _cellSize = 0.4f;
        [SerializeField] private float _opacity = 0.6f;

        private Tween _showTween;
        private Tween _hideTween;
        private DG.Tweening.Core.DOGetter<float> _getElapsed;
        private DG.Tweening.Core.DOSetter<float> _setShow;
        private DG.Tweening.Core.DOSetter<float> _setHide;

        private float _elapsed;
        private float _fromAlpha;
        private Vector3 _fromScale;
        private Vector3 _toScale;

        private void Awake()
        {
            var entity = CreateMetaEntity.Empty();
            entity.AddUnitRangeView(this);

            EnsureTweens();
        }

        public void ShowRangeView(Vector3 position, float range)
        {
            EnsureTweens();

            UpdateRangeView(position, range);

            _toScale = _rangeView.transform.localScale;
            _fromAlpha = _rangeView.color.a;

            _rangeView.gameObject.SetActive(true);
            _hideTween.Pause();
            _showTween.Restart();
        }

        public void UpdateRangeView(Vector3 position, float range)
        {
            transform.position = position;

            var physicalRadius = range * _cellSize;
            var targetScale = physicalRadius * 2f;

            _rangeView.transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }

        public void HideRangeView()
        {
            EnsureTweens();

            _fromScale = _rangeView.transform.localScale;
            _fromAlpha = _rangeView.color.a;

            _showTween.Pause();
            _hideTween.Restart();
        }

        private void OnDestroy()
        {
            if (_showTween != null && _showTween.active)
                _showTween.Kill();

            if (_hideTween != null && _hideTween.active)
                _hideTween.Kill();

            _showTween = null;
            _hideTween = null;
        }

        private void EnsureTweens()
        {
            _getElapsed ??= GetElapsed;
            _setShow ??= SetShow;
            _setHide ??= SetHide;

            if (_showTween == null || !_showTween.active)
                _showTween = CreateTween(_setShow, ShowScaleDuration);

            if (_hideTween == null || !_hideTween.active)
                _hideTween = CreateTween(_setHide, HideDuration);
        }

        private Tween CreateTween(DG.Tweening.Core.DOSetter<float> setter, float duration)
        {
            return DOTween.To(_getElapsed, setter, duration, duration)
                .From(0f, setImmediately: false)
                .SetEase(Ease.Linear)
                .SetAutoKill(false)
                .Pause();
        }

        private float GetElapsed() => _elapsed;

        private void SetShow(float elapsed)
        {
            _elapsed = elapsed;

            var scale = DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(elapsed / ShowScaleDuration), Ease.OutBack);
            _rangeView.transform.localScale = _toScale * scale;

            SetAlpha(DOVirtual.EasedValue(_fromAlpha, _opacity, Mathf.Clamp01(elapsed / ShowFadeDuration), Ease.OutQuad));
        }

        private void SetHide(float elapsed)
        {
            _elapsed = elapsed;

            var t = Mathf.Clamp01(elapsed / HideDuration);

            _rangeView.transform.localScale = Vector3.LerpUnclamped(_fromScale, Vector3.zero, DOVirtual.EasedValue(0f, 1f, t, Ease.InQuad));
            SetAlpha(DOVirtual.EasedValue(_fromAlpha, 0f, t, Ease.OutQuad));
        }

        private void SetAlpha(float alpha)
        {
            var color = _rangeView.color;
            color.a = alpha;
            _rangeView.color = color;
        }
    }
}
