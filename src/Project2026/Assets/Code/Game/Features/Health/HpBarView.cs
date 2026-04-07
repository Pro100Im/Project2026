using System.Collections;
using UnityEngine;

namespace Code.Game.Features.Health
{
    public class HpBarView : MonoBehaviour
    {
        [SerializeField] private Transform _fill;
        [SerializeField] private Transform _backFill;
        [Space]
        [SerializeField] private float _fullWidth = 0.58f;
        [SerializeField] private float _backDelaySpeed = 0.2f;

        private float _backVelocity;

        public void SetHp(float currentPercent)
        {
            _fill.localScale = new Vector3(_fullWidth * currentPercent, _fill.localScale.y, _fill.localScale.z);

            StopAllCoroutines();
            StartCoroutine(AnimateBackFill(currentPercent));
        }

        private IEnumerator AnimateBackFill(float targetPercent)
        {
            while (Mathf.Abs(_backFill.localScale.x - _fullWidth * targetPercent) > 0.001f)
            {
                var newX = Mathf.SmoothDamp(
                    _backFill.localScale.x,
                    _fullWidth * targetPercent,
                    ref _backVelocity,
                    _backDelaySpeed
                );

                _backFill.localScale = new Vector3(newX, _backFill.localScale.y, _backFill.localScale.z);

                yield return null;
            }
        }
    }
}