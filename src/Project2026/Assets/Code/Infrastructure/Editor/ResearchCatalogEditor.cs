#if UNITY_EDITOR
using Code.Game.StaticData.Configs;
using System;
using UnityEditor;
using UnityEngine;

namespace Code.Infrastructure.Editor
{
    [CustomEditor(typeof(ResearchCatalog))]
    public class ResearchCatalogEditor : UnityEditor.Editor
    {
        private SerializedObject _catalogObject;

        private void OnDisable()
        {
            _catalogObject?.Dispose();
            _catalogObject = null;
        }

        public override void OnInspectorGUI()
        {
            var catalog = (ResearchCatalog)target;
            var hadBranches = catalog.Branches != null && catalog.Branches.Length > 0;
            catalog.MigrateLegacyNodes();
            var migrated = !hadBranches && catalog.Branches != null && catalog.Branches.Length > 0;

            if (_catalogObject == null || _catalogObject.targetObject != catalog || migrated)
            {
                _catalogObject?.Dispose();
                _catalogObject = new SerializedObject(catalog);
            }

            _catalogObject.Update();

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_catalogObject.FindProperty("m_Script"));

            EditorGUILayout.PropertyField(
                _catalogObject.FindProperty("<NodeSpacing>k__BackingField"),
                new GUIContent("Node Spacing"));

            var branches = _catalogObject.FindProperty("<Branches>k__BackingField");
            DrawBranchList(branches);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_veteranKnight"));
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_eliteKnight"));
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_knightIcon"));
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_blockIcon"));
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_goldIcon"));
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_fireIcon"));
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_defenseIcon"));
            EditorGUILayout.PropertyField(_catalogObject.FindProperty("_archerIcon"));

            _catalogObject.ApplyModifiedProperties();
        }

        private void DrawBranchList(SerializedProperty branches)
        {
            EditorGUILayout.LabelField("Branches", EditorStyles.boldLabel);

            if (branches == null)
                return;

            for (var i = 0; i < branches.arraySize; i++)
                DrawBranch(branches.GetArrayElementAtIndex(i), i, branches, false);

            if (GUILayout.Button("Add Branch"))
            {
                AddRootBranch(branches);
                Commit();
            }
        }

        private void DrawBranch(SerializedProperty branch, int index, SerializedProperty list, bool managedList)
        {
            var name = branch.FindPropertyRelative("<Name>k__BackingField");
            var direction = branch.FindPropertyRelative("<Direction>k__BackingField");

            if (name == null || direction == null)
            {
                EditorGUILayout.PropertyField(branch, true);
                return;
            }

            var title = string.IsNullOrEmpty(name.stringValue) ? "Branch" : name.stringValue;
            var directionLabel = direction.enumValueIndex >= 0 && direction.enumValueIndex < direction.enumDisplayNames.Length
                ? direction.enumDisplayNames[direction.enumValueIndex]
                : string.Empty;

            EditorGUILayout.BeginHorizontal();
            branch.isExpanded = EditorGUILayout.Foldout(branch.isExpanded, title + "  " + directionLabel, true);
            var moved = DrawRowButtons(list, index, managedList);
            EditorGUILayout.EndHorizontal();

            if (moved)
                Commit();

            if (!branch.isExpanded)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(name, new GUIContent("Name"));
            EditorGUILayout.PropertyField(direction, new GUIContent("Direction"));

            var entries = branch.FindPropertyRelative("Entries");

            if (entries != null)
            {
                for (var i = 0; i < entries.arraySize; i++)
                    DrawEntry(entries.GetArrayElementAtIndex(i), i, entries);
            }

            EditorGUILayout.BeginHorizontal();
            var addNode = GUILayout.Button("Add Node");
            var addBranch = GUILayout.Button("Add Branch");
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            if (entries == null)
                return;

            if (addNode)
            {
                AddEntry(entries, new ResearchNodeDefinition());
                Commit();
            }

            if (addBranch)
            {
                AddEntry(entries, new ResearchBranchDefinition("Branch", ResearchLinkDirection.East, Array.Empty<ResearchBranchEntry>()));
                Commit();
            }
        }

        private void DrawEntry(SerializedProperty entry, int index, SerializedProperty list)
        {
            var typeName = entry.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(typeName))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Empty");
                var removed = DrawRowButtons(list, index, true);
                EditorGUILayout.EndHorizontal();

                if (removed)
                    Commit();

                return;
            }

            if (typeName.Contains(nameof(ResearchBranchDefinition)))
            {
                DrawBranch(entry, index, list, true);
                return;
            }

            var title = entry.FindPropertyRelative("<Title>k__BackingField");
            var label = title != null && !string.IsNullOrEmpty(title.stringValue) ? title.stringValue : "Node";

            EditorGUILayout.BeginHorizontal();
            entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, label, true);
            var moved = DrawRowButtons(list, index, true);
            EditorGUILayout.EndHorizontal();

            if (moved)
                Commit();

            if (!entry.isExpanded)
                return;

            EditorGUI.indentLevel++;
            DrawNodeFields(entry);
            EditorGUI.indentLevel--;
        }

        private static void DrawNodeFields(SerializedProperty node)
        {
            var iterator = node.Copy();
            var end = node.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                if (iterator.name is "<Parent>k__BackingField" or "<Direction>k__BackingField")
                    continue;

                EditorGUILayout.PropertyField(iterator, LabelFor(iterator), true);
            }
        }

        private static bool DrawRowButtons(SerializedProperty list, int index, bool managedList)
        {
            var changed = false;

            using (new EditorGUI.DisabledScope(index <= 0))
            {
                if (GUILayout.Button("Up", EditorStyles.miniButtonLeft, GUILayout.Width(36)))
                {
                    list.MoveArrayElement(index, index - 1);
                    changed = true;
                }
            }

            using (new EditorGUI.DisabledScope(index >= list.arraySize - 1))
            {
                if (GUILayout.Button("Down", EditorStyles.miniButtonMid, GUILayout.Width(48)))
                {
                    list.MoveArrayElement(index, index + 1);
                    changed = true;
                }
            }

            if (GUILayout.Button("Remove", EditorStyles.miniButtonRight, GUILayout.Width(64)))
            {
                if (managedList)
                    DeleteManaged(list, index);
                else
                    list.DeleteArrayElementAtIndex(index);

                changed = true;
            }

            return changed;
        }

        private void Commit()
        {
            _catalogObject.ApplyModifiedProperties();
            GUIUtility.ExitGUI();
        }

        private static void AddRootBranch(SerializedProperty branches)
        {
            var index = branches.arraySize;
            branches.arraySize++;
            var branch = branches.GetArrayElementAtIndex(index);
            branch.FindPropertyRelative("<Name>k__BackingField").stringValue = "Branch";
            branch.FindPropertyRelative("<Direction>k__BackingField").enumValueIndex = (int)ResearchLinkDirection.East;
            var entries = branch.FindPropertyRelative("Entries");

            if (entries != null)
                entries.arraySize = 0;
        }

        private static void AddEntry(SerializedProperty entries, ResearchBranchEntry entry)
        {
            var index = entries.arraySize;
            entries.arraySize++;
            entries.GetArrayElementAtIndex(index).managedReferenceValue = entry;
        }

        private static void DeleteManaged(SerializedProperty list, int index)
        {
            var size = list.arraySize;
            list.DeleteArrayElementAtIndex(index);

            if (list.arraySize == size && index < list.arraySize)
            {
                var element = list.GetArrayElementAtIndex(index);

                if (element.propertyType == SerializedPropertyType.ManagedReference && element.managedReferenceValue == null)
                    list.DeleteArrayElementAtIndex(index);
            }
        }

        private static GUIContent LabelFor(SerializedProperty property)
        {
            var name = property.name;
            var end = name.IndexOf('>');

            if (name.Length > 0 && name[0] == '<' && end > 1)
                name = name.Substring(1, end - 1);

            return new GUIContent(ObjectNames.NicifyVariableName(name));
        }
    }
}
#endif
