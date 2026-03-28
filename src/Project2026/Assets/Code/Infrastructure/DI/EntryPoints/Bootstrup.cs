using Code.Infrastructure.Loading;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class Bootstrup : IInitializable
    {
        private readonly ISceneLoader _sceneLoader;

        private readonly string _homeScreenSceneName;

        public Bootstrup(ISceneLoader sceneLoader, string homeScreenSceneName) 
        {
            _sceneLoader = sceneLoader;
            _homeScreenSceneName = homeScreenSceneName;
        }

        public void Initialize()
        {
            _sceneLoader.Load(_homeScreenSceneName);
        }
    }
}