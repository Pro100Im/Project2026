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

    private TransitionScreen _transitionScreen;
    private UIService _uIService;

    private VisualElement _canvas;
    private VisualElement _townMenu;

    private Button _exitButton;

    [Inject]
    public void Construct(TransitionScreen transitionScreen, UIService uIService)
    {
        _transitionScreen = transitionScreen;
        _uIService = uIService;
    }

    private void Awake()
    {
        var root = _townDoc.rootVisualElement;

        _canvas = root.Q<VisualElement>("Canvas");

        _exitButton = root.Q<Button>("ExitButton");

        _exitButton.clickable.clicked += ExitTown;
    }

    private void OnEnable()
    {
        //_uIService.Show(_canvas).Forget();
        //_transitionScreen.Hide().Forget();
    }

    private async void ExitTown()
    {
        await _transitionScreen.Show();

        try
        {
            for(var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.name != "Town")
                {
                    SceneManager.SetActiveScene(scene);

                    break;
                }
            }

            await _uIService.Hide(_canvas);
        }
        finally
        {
            await _transitionScreen.Hide();
        }
    }

    private void OnDestroy()
    {
        _exitButton.clickable.clicked -= ExitTown;
    }
}
