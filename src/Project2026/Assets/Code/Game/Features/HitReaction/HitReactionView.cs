using DG.Tweening;
using UnityEngine;

namespace Code.Game.Features.HitReaction
{
    /// <summary>
    /// Flash and punch on hit. One persistent tween per view drives both: it is created once and only restarted
    /// on every hit, so a hit allocates nothing.
    /// </summary>
    public class HitReactionView : MonoBehaviour
    {
        private const float FlashInShare = 0.45f;

        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _visual;
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private float _punchDistance = 0.055f;
        [SerializeField] private float _punchOutDuration = 0.04f;
        [SerializeField] private float _punchReturnDuration = 0.08f;

        private MaterialPropertyBlock _propertyBlock;

        // Animates elapsed seconds 0..total; the setter derives flash and punch from it.
        private Tween _tween;
        private DG.Tweening.Core.DOGetter<float> _getElapsed;
        private DG.Tweening.Core.DOSetter<float> _setElapsed;
        private TweenCallback _onComplete;

        private float _elapsed;
        private float _flashIn;
        private float _flashOut;
        private bool _playing;

        private Vector3 _visualRestLocalPosition;
        private Vector3 _punchedLocalPosition;
        private float _flashAmount;
        private bool _restCached;

        private void Awake()
        {
            CacheRest();
            SetFlash(0f);
            EnsureTween();
        }

        public void Play(Vector2 direction)
        {
            if (!isActiveAndEnabled || _spriteRenderer == null)
                return;

            CacheRest();
            EnsureTween();

            var worldDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            var worldOffset = new Vector3(worldDirection.x, worldDirection.y, 0f) * _punchDistance;
            var localOffset = _visual != null ? _visual.InverseTransformVector(worldOffset) : Vector3.zero;
            localOffset.z = 0f;

            _punchedLocalPosition = _visualRestLocalPosition + localOffset;
            _playing = true;
            _tween.Restart();
        }

        public void Tick(float deltaTime)
        {
            if (!_playing || deltaTime <= 0f)
                return;

            _tween.ManualUpdate(deltaTime, deltaTime);
        }

        public void Stop()
        {
            if (_tween != null && _tween.active)
                _tween.Pause();

            Finish();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            _playing = false;

            if (_tween != null && _tween.active)
                _tween.Kill();

            _tween = null;
        }

        private void EnsureTween()
        {
            if (_tween != null && _tween.active)
                return;

            _flashIn = _flashDuration * FlashInShare;
            _flashOut = Mathf.Max(0.01f, _flashDuration - _flashIn);

            var total = Mathf.Max(_flashIn + _flashOut, _punchOutDuration + _punchReturnDuration);

            _getElapsed ??= GetElapsed;
            _setElapsed ??= SetElapsed;
            _onComplete ??= Finish;

            _tween = DOTween.To(_getElapsed, _setElapsed, total, total)
                .From(0f)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Manual)
                .SetAutoKill(false)
                .OnComplete(_onComplete)
                .Pause();
        }

        private float GetElapsed() => _elapsed;

        private void SetElapsed(float elapsed)
        {
            _elapsed = elapsed;

            if (!_playing)
                return;

            SetFlash(EvaluateFlash(elapsed));

            if (_visual != null && _restCached)
                _visual.localPosition = EvaluatePunch(elapsed);
        }

        private float EvaluateFlash(float elapsed)
        {
            if (elapsed < _flashIn)
                return DOVirtual.EasedValue(0f, 1f, elapsed / _flashIn, Ease.OutQuad);

            var outTime = elapsed - _flashIn;

            return outTime < _flashOut
                ? DOVirtual.EasedValue(1f, 0f, outTime / _flashOut, Ease.InQuad)
                : 0f;
        }

        private Vector3 EvaluatePunch(float elapsed)
        {
            if (elapsed < _punchOutDuration)
            {
                var t = DOVirtual.EasedValue(0f, 1f, elapsed / _punchOutDuration, Ease.OutQuad);
                return Vector3.LerpUnclamped(_visualRestLocalPosition, _punchedLocalPosition, t);
            }

            var returnTime = elapsed - _punchOutDuration;

            if (_punchReturnDuration > 0f && returnTime < _punchReturnDuration)
            {
                var t = DOVirtual.EasedValue(0f, 1f, returnTime / _punchReturnDuration, Ease.OutQuad);
                return Vector3.LerpUnclamped(_punchedLocalPosition, _visualRestLocalPosition, t);
            }

            return _visualRestLocalPosition;
        }

        private void Finish()
        {
            _playing = false;
            SetFlash(0f);
            RestoreVisualPosition();
        }

        private void CacheRest()
        {
            if (_restCached || _visual == null)
                return;

            _visualRestLocalPosition = _visual.localPosition;
            _restCached = true;
        }

        private void RestoreVisualPosition()
        {
            if (_visual == null || !_restCached)
                return;

            _visual.localPosition = _visualRestLocalPosition;
        }

        private void SetFlash(float amount)
        {
            _flashAmount = amount;

            if (_spriteRenderer == null)
                return;

            _propertyBlock ??= new MaterialPropertyBlock();

            _spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(FlashAmountId, _flashAmount);
            _spriteRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
