using Code.Infrastructure.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class Bootstrup : IInitializable
    {
        private readonly ISceneLoader _sceneLoader;

        private readonly string _homeScreenSceneName;
        private readonly string _townSceneName;

        public Bootstrup(ISceneLoader sceneLoader, string homeScreenSceneName, string townSceneName) 
        {
            _sceneLoader = sceneLoader;
            _homeScreenSceneName = homeScreenSceneName;
            _townSceneName = townSceneName;
        }

        public void Initialize()
        {
            _sceneLoader.Load(_homeScreenSceneName, LoadSceneMode.Additive).Forget();
            _sceneLoader.Load(_townSceneName, LoadSceneMode.Additive).Forget();
        }
    }
}