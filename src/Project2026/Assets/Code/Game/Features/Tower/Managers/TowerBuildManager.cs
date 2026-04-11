using Code.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Code.Game.Features.Tower.Managers
{
    public class TowerBuildManager : MonoBehaviour
    {
        [SerializeField] private UIDocument _doc;

        private UIService _uIService;

        private VisualElement _towerBuilds;

        private Button _openButton;
        private Button _closeButton;

        [Inject]
        private void Construct(UIService uIService)
        {
            _uIService = uIService;
        }

        private void Start()
        {
            var root = _doc.rootVisualElement;

            _towerBuilds = root.Q<VisualElement>("TowerBuilds");

            _openButton = root.Q<Button>("OpenButton");
            _closeButton = root.Q<Button>("CloseButton");

            _openButton.clickable.clicked += Open;
            _closeButton.clickable.clicked += Close;
        }

        private async void Open()
        {
            _openButton.pickingMode = PickingMode.Ignore;

            await _uIService.Show(_towerBuilds);

            _closeButton.pickingMode = PickingMode.Position;
        }

        private async void Close()
        {
            _closeButton.pickingMode = PickingMode.Ignore;

            await _uIService.Hide(_towerBuilds);

            _openButton.pickingMode = PickingMode.Position;
        }

        private void OnDestroy()
        {
            _openButton.clickable.clicked -= Open;
            _closeButton.clickable.clicked -= Close;
        }
    }
}