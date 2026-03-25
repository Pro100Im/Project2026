using System;

namespace Code.Infrastructure.Loading
{
    public interface ISceneLoader
    {
        public void Load(string name, Action onLoaded = null);
    }
}