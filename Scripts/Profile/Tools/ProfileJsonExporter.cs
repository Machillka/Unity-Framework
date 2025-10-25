#if UNITY_EDITOR
using System.IO;
using Framework.Profile.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Framework.Profile.Tools
{
    public static class ProfileJsonExporter
    {
        public static void ExportToFile(BaseProfileSO profile, string path)
        {
            if (profile == null)
                throw new System.ArgumentNullException(nameof(profile));
            var json = JsonUtility.ToJson(profile, true);
            File.WriteAllText(path, json);
            if (path.StartsWith(Application.dataPath)) AssetDatabase.Refresh();
        }
    }
}
#endif