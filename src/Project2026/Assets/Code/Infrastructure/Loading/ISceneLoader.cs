using System;

namespace Code.Infrastructure.Loading
{
    public interface ISceneLoader
    {
        public void LocalLoad(string name, Action onLoaded = null);
        public void NetworkLoad(string nextScene);
    }
}