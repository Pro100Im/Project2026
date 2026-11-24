#if UNITY_EDITOR
using Code.Infrastructure.DI.LifetimeScopes;
using Code.Infrastructure.Persistence;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Code.Infrastructure.Editor
{
    public static class LevelProgressDebugMenu
    {
        private static readonly List<MetaEntity> NodesBuffer = new(16);

        [MenuItem("Tools/Levels/Reset Level Progress", false, 20)]
        private static void ResetLevelProgress()
        {
            if (TryGetLiveProgress(out var progress))
            {
                progress.ResetLevels();

                var nodes = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.LevelNode).GetEntities(NodesBuffer);

                for (var i = 0; i < nodes.Count; i++)
                {
                    nodes[i].isLevelNodeCompleted = false;
                    nodes[i].ReplaceLevelNodeStars(0);
                }

                var menu = Contexts.sharedInstance.meta.GetGroup(MetaMatcher.LevelMapMenu).GetSingleEntity();

                if (menu != null)
                    menu.isLevelMapDirty = true;

                return;
            }

            var storage = new JsonFileStorageService();

            if (!storage.TryLoad<UserSaveData>(UserProgressService.SaveKey, out var save) || save?.Campaign?.Levels == null)
                return;

            save.Campaign.Levels.Clear();
            save.Campaign.CompletedLevels = null;
            storage.Save(UserProgressService.SaveKey, save);
        }

        private static bool TryGetLiveProgress(out IUserProgressService progress)
        {
            progress = null;

            if (!Application.isPlaying)
                return false;

            var scopes = Object.FindObjectsByType<GlobalScope>(FindObjectsInactive.Include);

            for (var i = 0; i < scopes.Length; i++)
            {
                var container = scopes[i].Container;

                if (container != null && container.TryResolve(out progress) && progress != null)
                    return true;
            }

            return false;
        }
    }
}
#endif
