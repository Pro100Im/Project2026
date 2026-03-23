using Code.Common.StaticData;
using Code.Infrastructure.Loading;
using VContainer.Unity;

namespace Code.Infrastructure.DI.EntryPoints
{
    public class Bootstrup : IInitializable
    {
        private readonly IStaticDataService _staticDataService;
        private readonly ISceneLoader _sceneLoader;

        private readonly string _homeScreenSceneName;

        public Bootstrup(IStaticDataService staticDataService, ISceneLoader sceneLoader, string homeScreenSceneName) 
        {
            _staticDataService = staticDataService;
            _sceneLoader = sceneLoader;
            _homeScreenSceneName = homeScreenSceneName;
        }

        public void Initialize()
        {
            _staticDataService.LoadAll();
            _sceneLoader.LocalLoad(_homeScreenSceneName);
        }
    }
}