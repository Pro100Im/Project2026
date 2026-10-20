#if UNITY_EDITOR
using Code.Game.Features.Town;
using Code.Infrastructure.DI.LifetimeScopes;
using Code.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Code.Infrastructure.Editor
{
    public static class TownProgressDebugMenu
    {
        private static readonly List<TownEntity> BuildingsBuffer = new(16);

        [MenuItem("Tools/Town/Reset Player", false, 1)]
        private static void ResetPlayer() => Reset(true, true, true);

        [MenuItem("Tools/Town/Reset Resources", false, 2)]
        private static void ResetResources() => Reset(true, false, false);

        [MenuItem("Tools/Town/Reset Buildings", false, 3)]
        private static void ResetBuildings() => Reset(false, true, false);

        [MenuItem("Tools/Town/Reset Research", false, 4)]
        private static void ResetResearch() => Reset(false, false, true);

        private static void Reset(bool resetResources, bool resetBuildings, bool resetResearch)
        {
            if (TryGetLiveProgress(out var progress))
            {
                if (resetResources && resetBuildings && resetResearch)
                    progress.ResetPlayer();
                else
                {
                    if (resetResources)
                        progress.ResetResources();

                    if (resetBuildings)
                        progress.ResetBuildings();

                    if (resetResearch)
                        progress.ResetResearch();
                }

                ApplyLiveTown(resetResources, resetBuildings, resetResearch);
                return;
            }

            ResetFile(resetResources, resetBuildings, resetResearch);
        }

        private static bool TryGetLiveProgress(out IUserProgressService progress)
        {
            progress = null;

            if (!Application.isPlaying)
                return false;

            var scopes = UnityEngine.Object.FindObjectsByType<GlobalScope>(FindObjectsInactive.Include);

            for (var i = 0; i < scopes.Length; i++)
            {
                var container = scopes[i].Container;

                if (container == null || !container.TryResolve(out progress) || progress == null)
                    continue;

                return true;
            }

            return false;
        }

        private static void ResetFile(bool resetResources, bool resetBuildings, bool resetResearch)
        {
            var storage = new JsonFileStorageService();

            if (!storage.TryLoad<UserSaveData>(UserProgressService.SaveKey, out var save) || save == null)
            {
                save = new UserSaveData
                {
                    Version = UserSaveData.CurrentVersion,
                    UserId = Guid.NewGuid().ToString("N"),
                    Town = new TownProgressData()
                };
            }

            save.Town ??= new TownProgressData();
            save.Town.Buildings ??= new List<TownBuildingProgressData>(8);
            save.Town.Research ??= new List<ResearchProgressData>(8);

            if (resetResources)
                save.Town.ClearResources();

            if (resetBuildings)
                save.Town.ClearBuildings();

            if (resetResearch)
                save.Town.ClearResearch();

            storage.Save(UserProgressService.SaveKey, save);
        }

        private static void ApplyLiveTown(bool resetResources, bool resetBuildings, bool resetResearch)
        {
            var town = Contexts.sharedInstance.town;

            if (resetResources)
                ApplyLiveResources(town);

            if (resetBuildings)
                ApplyLiveBuildings(town);

            if (resetResearch)
            {
                ApplyLiveResearch(town);
                ApplyLiveGameResearch();
            }
        }

        private static void ApplyLiveResources(TownContext town)
        {
            var exchequer = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownExchequer,
                    TownMatcher.TownResources,
                    TownMatcher.TownGold,
                    TownMatcher.TownStone,
                    TownMatcher.TownCrystal,
                    TownMatcher.TownWood,
                    TownMatcher.TownKnowledge)).GetSingleEntity();

            if (exchequer == null)
                return;

            exchequer.ReplaceTownGold(0);
            exchequer.ReplaceTownStone(0);
            exchequer.ReplaceTownCrystal(0);
            exchequer.ReplaceTownWood(0);
            exchequer.ReplaceTownKnowledge(0);

            var view = exchequer.townExchequer.Value;

            if (view == null)
                return;

            view.SetGold(0, false);
            view.SetStone(0, false);
            view.SetCrystal(0, false);
            view.SetWood(0, false);
            view.SetKnowledge(0, false);
        }

        private static void ApplyLiveBuildings(TownContext town)
        {
            var buildings = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.TownBuilding,
                    TownMatcher.TownBuildingLevel)).GetEntities(BuildingsBuffer);

            for (var i = 0; i < buildings.Count; i++)
            {
                if (buildings[i].townBuildingLevel.Value == 0)
                    continue;

                buildings[i].ReplaceTownBuildingLevel(0);
            }
        }

        private static readonly List<TownEntity> ResearchBuffer = new(32);
        private static readonly List<GameEntity> GameResearchBuffer = new(32);

        private static void ApplyLiveResearch(TownContext town)
        {
            var nodes = town.GetGroup(TownMatcher
                .AllOf(
                    TownMatcher.ResearchNode,
                    TownMatcher.ResearchLevel,
                    TownMatcher.ResearchStartingLevel)).GetEntities(ResearchBuffer);

            for (var i = 0; i < nodes.Count; i++)
            {
                var start = nodes[i].researchStartingLevel.Value;

                if (nodes[i].researchLevel.Value == start)
                    continue;

                nodes[i].ReplaceResearchLevel(start);
            }
        }

        private static void ApplyLiveGameResearch()
        {
            var nodes = Contexts.sharedInstance.game.GetGroup(GameMatcher
                .AllOf(
                    GameMatcher.ResearchNode,
                    GameMatcher.ResearchLevel,
                    GameMatcher.ResearchStartingLevel)).GetEntities(GameResearchBuffer);

            for (var i = 0; i < nodes.Count; i++)
            {
                var start = nodes[i].researchStartingLevel.Value;

                if (nodes[i].researchLevel.Value == start)
                    continue;

                nodes[i].ReplaceResearchLevel(start);
            }
        }
    }
}
#endif
