using UnityEngine;

namespace Code.Game.Features.Player.Service
{
    public class PlayerAnimatorService : MonoBehaviour
    {
        [SerializeField] private string _verticalParameter = "Vertical";
        [SerializeField] private string _horizontalParameter = "Horizontal";
        [Space]
        [SerializeField] private Animator _animator;

        public void SetMoveParameters(float y, float x)
        {
            _animator?.SetFloat(_verticalParameter, y);
            _animator?.SetFloat(_horizontalParameter, x);
        }
    }
}