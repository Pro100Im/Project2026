using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Code.Infrastructure.Loading
{
    public interface ISceneLoader
    {
        public UniTask Load(string nextScene, LoadSceneMode mode = LoadSceneMode.Single, CancellationToken token = default);
        public UniTask UnLoad(string sceneName);
    }
}