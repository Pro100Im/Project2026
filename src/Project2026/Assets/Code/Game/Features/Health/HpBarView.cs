using DG.Tweening;
using UnityEngine;

namespace Code.Game.Features.Health
{
    public class HpBarView : MonoBehaviour
    {
        [SerializeField] private Transform _fill;
        [SerializeField] private Transform _backFill;
        [Space]
        [SerializeField] private float _fullWidth = 0.58f;
        [SerializeField] private float _backDelaySpeed = 0.3f;

        private Tween _backTween;

        public void SetHp(float currentPercent)
        {
            _fill.localScale = new Vector3(_fullWidth * currentPercent, _fill.localScale.y, _fill.localScale.z);

            _backTween?.Kill();
            _backTween = _backFill.DOScaleX(_fullWidth * currentPercent, _backDelaySpeed).SetEase(Ease.OutSine);
        }

        private void OnDestroy()
        {
            _backTween?.Kill();
        }
    }
}