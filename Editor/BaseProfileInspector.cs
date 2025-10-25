#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Framework.Profile.Scriptable;

namespace Framework.Profile.Editor
{
    [CustomEditor(typeof(BaseProfileSO), true)]
    public class BaseProfileInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default inspector for profile-specific fields
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Validate Profile"))
            {
                var bp = (BaseProfileSO)target;
                var err = bp.Validate();
                if (string.IsNullOrEmpty(err))
                {
                    EditorUtility.DisplayDialog("Profile Validate", "Profile valid", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Profile Validate", "Validation failed:\n" + err, "OK");
                }
            }

            if (GUILayout.Button("Export JSON"))
            {
                var path = EditorUtility.SaveFilePanel("Export Profile JSON", Application.dataPath, target.name + ".json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    Framework.Profile.Tools.ProfileJsonExporter.ExportToFile((BaseProfileSO)target, path);
                    EditorUtility.DisplayDialog("Export", "Profile exported to:\n" + path, "OK");
                }
            }

            if (GUILayout.Button("Import JSON (overwrite)"))
            {
                var path = EditorUtility.OpenFilePanel("Import Profile JSON", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    bool ok = Framework.Profile.Tools.ProfileJsonImporter.ImportFromFile((BaseProfileSO)target, path);
                    EditorUtility.DisplayDialog("Import", ok ? "Import succeeded" : "Import failed", "OK");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
