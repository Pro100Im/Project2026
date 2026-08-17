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
        private readonly string _bootstrupSceneName;

        public Bootstrup(ISceneLoader sceneLoader, string homeScreenSceneName, string townSceneName, string bootstrupSceneName) 
        {
            _sceneLoader = sceneLoader;
            _homeScreenSceneName = homeScreenSceneName;
            _townSceneName = townSceneName;
            _bootstrupSceneName = bootstrupSceneName;
        }

        public async void Initialize()
        {
            await UniTask.WhenAll(_sceneLoader.Load(_homeScreenSceneName, LoadSceneMode.Additive), _sceneLoader.Load(_townSceneName, LoadSceneMode.Additive));

            _sceneLoader.UnLoad(_bootstrupSceneName).Forget();
        }
    }
}