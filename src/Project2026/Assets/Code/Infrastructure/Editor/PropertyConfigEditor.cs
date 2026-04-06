#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Code.Game.StaticData.Configs;
using Code.Game.StaticData;

namespace Assets.Code.Infrastructure
{
    [CustomEditor(typeof(EntityConfig))]
    public class PropertyConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var config = (EntityConfig)target;

            if (config.Properties == null)
                config.Properties = new EntityProperty[0];

            if (GUILayout.Button("Add Property"))
            {
                var menu = new GenericMenu();
                var types = typeof(EntityProperty).Assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(EntityProperty)) && !t.IsAbstract);

                foreach (var type in types)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        Array.Resize(ref config.Properties, config.Properties.Length + 1);
                        config.Properties[config.Properties.Length - 1] = (EntityProperty)Activator.CreateInstance(type);
                        EditorUtility.SetDirty(config);
                    });
                }

                menu.ShowAsContext();
            }

            for (int i = 0; i < config.Properties.Length; i++)
            {
                var prop = config.Properties[i];

                if (prop == null) 
                    continue;

                EditorGUILayout.LabelField(prop.GetType().Name, EditorStyles.boldLabel);

                var so = new SerializedObject(config);
                var arrayProp = so.FindProperty("Properties");
                var element = arrayProp.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(element, true);

                so.ApplyModifiedProperties();

                if (GUILayout.Button("Remove"))
                {
                    var list = config.Properties.ToList();

                    list.RemoveAt(i);
                    config.Properties = list.ToArray();

                    EditorUtility.SetDirty(config);
                }
            }
        }
    }
}
#endif