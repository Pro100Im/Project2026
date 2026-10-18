using DG.Tweening;
using UnityEngine;

namespace Code.Game.Features.HitReaction
{
    public class HitReactionView : MonoBehaviour
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _visual;
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private float _punchDistance = 0.055f;
        [SerializeField] private float _punchOutDuration = 0.04f;
        [SerializeField] private float _punchReturnDuration = 0.08f;

        private MaterialPropertyBlock _propertyBlock;

        private Sequence _sequence;
        private Vector3 _visualRestLocalPosition;
        private float _flashAmount;
        private bool _restCached;
        private bool _isStopping;

        private void Awake()
        {
            CacheRest();
            SetFlash(0f);
        }

        public void Play(Vector2 direction)
        {
            if (!isActiveAndEnabled || _spriteRenderer == null)
                return;

            CacheRest();
            Stop();

            var worldDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            var worldOffset = new Vector3(worldDirection.x, worldDirection.y, 0f) * _punchDistance;
            var localOffset = _visual != null ? _visual.InverseTransformVector(worldOffset) : Vector3.zero;
            localOffset.z = 0f;

            var flashInDuration = _flashDuration * 0.45f;
            var flashOutDuration = Mathf.Max(0.01f, _flashDuration - flashInDuration);
            var flashIn = DOTween.To(GetFlash, SetFlash, 1f, flashInDuration).SetEase(Ease.OutQuad).Pause();
            var flashOut = DOTween.To(GetFlash, SetFlash, 0f, flashOutDuration).SetEase(Ease.InQuad).Pause();

            _sequence = DOTween.Sequence().SetUpdate(UpdateType.Manual);
            _sequence.Append(flashIn);
            _sequence.Append(flashOut);

            if (_visual != null)
            {
                var punchedPosition = _visualRestLocalPosition + localOffset;
                var punchOut = _visual.DOLocalMove(punchedPosition, _punchOutDuration).SetEase(Ease.OutQuad).Pause();
                var punchReturn = _visual.DOLocalMove(_visualRestLocalPosition, _punchReturnDuration).SetEase(Ease.OutQuad).Pause();

                _sequence.Insert(0f, punchOut);
                _sequence.Insert(_punchOutDuration, punchReturn);
            }

            _sequence.OnComplete(Finish);
        }

        public void Tick(float deltaTime)
        {
            if (_sequence == null || !_sequence.active || deltaTime <= 0f)
                return;

            _sequence.ManualUpdate(deltaTime, deltaTime);
        }

        public void Stop()
        {
            if (_isStopping)
                return;

            _isStopping = true;

            if (_sequence != null)
            {
                var sequence = _sequence;
                _sequence = null;
                sequence.OnComplete(null);

                if (sequence.active)
                    sequence.Kill(false);
            }

            SetFlash(0f);
            RestoreVisualPosition();
            _isStopping = false;
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void Finish()
        {
            _sequence = null;
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

        private float GetFlash() => _flashAmount;

        private void SetFlash(float amount)
        {
            _flashAmount = amount;

            if (_spriteRenderer == null)
                return;

            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(FlashAmountId, _flashAmount);
            _spriteRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
