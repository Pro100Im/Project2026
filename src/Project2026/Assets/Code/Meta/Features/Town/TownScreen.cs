using Code.Game.Common.Cameras;
using Code.Game.Common.UI;
using Code.Game.Common.UI.Transition;
using Cysharp.Threading.Tasks;
using System;
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

    private VisualElement _canvas;
    private VisualElement _townMenu;

    private Button _exitButton;

    [Inject]
    public void Construct(TransitionScreen transitionScreen, UIService uIService, ICameraService cameraService)
    {
        _transitionScreen = transitionScreen;
        _uIService = uIService;
        _cameraService = cameraService;
    }

    private void Awake()
    {
        var root = _townDoc.rootVisualElement;

        _canvas = root.Q<VisualElement>("Canvas");
        _exitButton = root.Q<Button>("ExitButton");

        _exitButton.clickable.clicked += ExitTown;

        _uIService.Hide(_canvas).Forget();
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        if (SceneManager.GetActiveScene().name.Equals(_townSceneName))
        {
            _uIService.Show(_canvas).Forget();
            _transitionScreen.Hide().Forget();
        }
        else if(!_uIService.HasComponent(_canvas, "hide"))
            _uIService.Hide(_canvas).Forget();
    }

    private async void ExitTown()
    {
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
        _exitButton.clickable.clicked -= ExitTown;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
}
