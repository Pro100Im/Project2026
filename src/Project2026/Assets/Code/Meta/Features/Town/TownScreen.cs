using Code.Game.Common.Cameras;
using Code.Game.Common.Entity;
using Code.Game.Common.UI;
using Code.Game.Common.UI.Transition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;

public class TownScreen : MonoBehaviour
{
    [SerializeField] private string _townSceneName = "Town";
    [Space]
    [SerializeField] private UIDocument _townDoc;

    private ICameraService _cameraService;

    private TransitionScreen _transitionScreen;
    private UIService _uIService;

    private VisualElement _root;
    private VisualElement _canvas;

    private Button _exitButton;
    private Button _townButton;

    [Inject]
    public void Construct(TransitionScreen transitionScreen, UIService uIService, ICameraService cameraService)
    {
        _transitionScreen = transitionScreen;
        _uIService = uIService;
        _cameraService = cameraService;
    }

    private void Awake()
    {
        _root = _townDoc.rootVisualElement;

        _canvas = _root.Q<VisualElement>("TowerCanvas");
        _exitButton = _root.Q<Button>("ExitButton");
        _townButton = _root.Q<Button>("TownButton");

        _exitButton.clickable.clicked += ExitTown;
        _townButton.clickable.clicked += ToggleBuildMenuRequest;

        _uIService.Hide(_canvas).Forget();
    }

    private void Start()
    {
        _uIService.RegisterRoot(_root);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    public VisualElement GetRoot() => _root;

    public VisualElement GetVisualElement(string name) => _root.Q<VisualElement>(name);

    public Button GetButton(string name) => _root.Q<Button>(name);
    public Image GetImage(string name) => _root.Q<Image>(name);
    public Label GetLabel(string name) => _root.Q<Label>(name);

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        if (SceneManager.GetActiveScene().name.Equals(_townSceneName))
        {
            _uIService.Show(_canvas).Forget();
            _transitionScreen.Hide().Forget();
        }
        else if(!_uIService.HasComponent(_canvas, "hide"))
        {
            _uIService.Hide(_canvas).Forget();
        }
    }

    private void ToggleBuildMenuRequest()
    {
        var request = CreateInputEntity.Empty();
        request.isTownBuildMenuToggleRequested = true;
        request.isInput = true;
    }

    private void CloseBuildMenuRequest()
    {
        var request = CreateInputEntity.Empty();
        request.isTownBuildMenuCloseRequested = true;
        request.isInput = true;
    }

    private async void ExitTown()
    {
        CloseBuildMenuRequest();

        await _transitionScreen.Show();

        _cameraService.SetActiveMainCamera();

        try
        {
            for(var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (!scene.name.Equals(_townSceneName))
                {
                    SceneManager.SetActiveScene(scene);

                    break;
                }
            }
        }
        catch
        {
            
        }
    }

    private void OnDestroy()
    {
        _uIService?.UnregisterRoot(_root);
        _exitButton.clickable.clicked -= ExitTown;
        _townButton.clickable.clicked -= ToggleBuildMenuRequest;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
}
