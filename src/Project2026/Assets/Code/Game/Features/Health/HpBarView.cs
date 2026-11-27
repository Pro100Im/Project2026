using DG.Tweening;
using UnityEngine;

namespace Code.Game.Features.Health
{
    /// <summary>
    /// The back fill catches up with the front fill after a delay. One persistent tween per bar, restarted on
    /// every change, so taking damage allocates nothing.
    /// </summary>
    public class HpBarView : MonoBehaviour
    {
        [SerializeField] private Transform _fill;
        [SerializeField] private Transform _backFill;
        [Space]
        [SerializeField] private float _fullWidth = 0.58f;
        [SerializeField] private float _backDelaySpeed = 0.3f;

        private Tween _backTween;
        private DG.Tweening.Core.DOGetter<float> _getProgress;
        private DG.Tweening.Core.DOSetter<float> _setProgress;

        private float _progress;
        private float _backFrom;
        private float _backTo;

        private void Awake()
        {
            EnsureTween();
        }

        public void SetHp(float currentPercent)
        {
            var width = _fullWidth * currentPercent;

            _fill.localScale = new Vector3(width, _fill.localScale.y, _fill.localScale.z);

            EnsureTween();

            _backFrom = _backFill.localScale.x;
            _backTo = width;
            _backTween.Restart();
        }

        private void OnDisable()
        {
            if (_backTween == null || !_backTween.active)
                return;

            _backTween.Pause();
            SetBackWidth(_backTo);
        }

        private void OnDestroy()
        {
            if (_backTween != null && _backTween.active)
                _backTween.Kill();

            _backTween = null;
        }

        private void EnsureTween()
        {
            if (_backTween != null && _backTween.active)
                return;

            _getProgress ??= GetProgress;
            _setProgress ??= SetProgress;
            _backTo = _backFill.localScale.x;
            _backFrom = _backTo;

            _backTween = DOTween.To(_getProgress, _setProgress, 1f, _backDelaySpeed)
                .From(0f)
                .SetDelay(_backDelaySpeed)
                .SetEase(Ease.Linear)
                .SetAutoKill(false)
                .Pause();
        }

        private float GetProgress() => _progress;

        private void SetProgress(float progress)
        {
            _progress = progress;
            SetBackWidth(Mathf.LerpUnclamped(_backFrom, _backTo, DOVirtual.EasedValue(0f, 1f, progress, Ease.InSine)));
        }

        private void SetBackWidth(float width)
        {
            var scale = _backFill.localScale;

            if (scale.x == width)
                return;

            scale.x = width;
            _backFill.localScale = scale;
        }
    }
}
