using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Infrastructure.Loading
{
    public class SceneLoader : ISceneLoader
    {
        public async UniTask Load(string nextScene, LoadSceneMode mode = LoadSceneMode.Single, CancellationToken token = default, bool setActiveScene = false)
        {
            var scene = SceneManager.GetSceneByName(nextScene);

            if (scene.IsValid() && scene.isLoaded) 
                return;

            try
            {
                await SceneManager.LoadSceneAsync(nextScene, mode).ToUniTask(cancellationToken: token);

                if(setActiveScene)
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextScene));
            }
            catch (OperationCanceledException)
            {
                await SceneManager.UnloadSceneAsync(nextScene);

                throw;
            }
        }

        public async UniTask UnLoad(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid() || !scene.isLoaded) 
                return;

            await SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}