using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Infrastructure.Helpers
{
    // Has execution order to start before every other script // TO DO
    public class SwitchToEntrySceneInEditor : MonoBehaviour
    {
#if UNITY_EDITOR
        private const int EntrySceneIndex = 0;

        private void Awake()
        {
            //if (ProjectContext.HasInstance) 
            //  return;

            //foreach (GameObject root in gameObject.scene.GetRootGameObjects()) 
            //  root.SetActive(false);

            //SceneManager.LoadScene(EntrySceneIndex);
        }
#endif
    }
}