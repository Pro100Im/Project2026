using UnityEngine;

namespace Code.Infrastructure.Persistence
{
    public class ProgressSaveHook : MonoBehaviour
    {
        private IUserProgressService _progress;

        public void Bind(IUserProgressService progress) => _progress = progress;

        public void Unbind() => _progress = null;

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                _progress?.Flush();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                _progress?.Flush();
        }

        private void OnApplicationQuit() => _progress?.Flush();
    }
}
