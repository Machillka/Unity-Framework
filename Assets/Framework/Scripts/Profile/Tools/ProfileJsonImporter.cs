#if UNITY_EDITOR
using System.IO;
using Framework.Profile.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Framework.Profile.Tools
{
    public static class ProfileJsonImporter
    {
        public static bool ImportFromFile(BaseProfileSO profile, string path)
        {
            if (profile == null)
                throw new System.ArgumentNullException(nameof(profile));
            if (!File.Exists(path))
                return false;

            var json = File.ReadAllText(path);
            try
            {
                JsonUtility.FromJsonOverwrite(json, profile);
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
                return true;
            }
            catch (System.Exception exp)
            {
                Debug.LogException(exp);
                return false;
            }
        }
    }
}
#endif