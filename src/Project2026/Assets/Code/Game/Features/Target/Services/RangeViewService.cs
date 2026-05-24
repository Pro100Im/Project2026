using DG.Tweening;
using UnityEngine;

namespace Code.Game.Features.Target.Services
{
    public class RangeViewService : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _rangeView;
        [SerializeField] private float _cellSize = 0.4f;
        [SerializeField] private float _opacity = 0.6f;

        private Tween _fadeTween;
        private Tween _scaleTween;

        public void ShowRangeView(Vector3 position, float range)
        {
            _fadeTween?.Kill();
            _scaleTween?.Kill();

            transform.position = position;

            var physicalRadius = range * _cellSize;
            var targetScale = physicalRadius * 2f;
            var finalScale = new Vector3(targetScale, targetScale, 1f);

            _rangeView.gameObject.SetActive(true);
            _fadeTween = _rangeView.DOFade(_opacity, 0.25f);
            _scaleTween = _rangeView.transform.DOScale(finalScale, 0.3f)
                .From(Vector3.zero)
                .SetEase(Ease.OutBack);
        }

        public void HideRangeView()
        {
            _fadeTween?.Kill();
            _scaleTween?.Kill();

            _scaleTween = _rangeView.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InQuad);
            _fadeTween = _rangeView.DOFade(0f, 0.2f).OnComplete(() =>
            {
                _rangeView.gameObject.SetActive(false);
            });
        }
    }
}