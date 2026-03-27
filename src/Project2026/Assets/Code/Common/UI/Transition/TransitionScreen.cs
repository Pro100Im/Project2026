using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Common.UI.Transition
{
    public class TransitionScreen : MonoBehaviour
    {
        [SerializeField] private UIDocument _transitionScreenDoc;

        private VisualElement _canvas;
        private UIService _uIService;

        [Inject]
        private void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Awake()
        {
            var root = _transitionScreenDoc.rootVisualElement;
            _canvas = root.Q<VisualElement>("Canvas");
        }

        public async UniTask Show()
        {
            await UniTask.WaitForEndOfFrame();
            await _uIService.Show(_canvas);
        }

        public async UniTask Hide()
        {
            await UniTask.WaitForEndOfFrame();
            await _uIService.Hide(_canvas);
        }
    }
}