using System.Collections.Generic;
using UnityEngine;

namespace Framework.Utilities.AudioManager
{
    [CreateAssetMenu(fileName = "AudioCatalog", menuName = "Framework/Audio/Audio Catalog")]
    public class AudioCatalogSO : ScriptableObject
    {
        public List<SoundEntry> entries = new();
        public Dictionary<string, SoundEntry> entriesDict = new();

        // NOTE: 在编辑器中保持同步，以及统计数量一样不代表内容一致
        private void EnsureMap()
        {
            if (entries.Count != entriesDict.Count)
            {
                entriesDict.Clear();
                foreach (var e in entries)
                {
                    if (!entriesDict.ContainsKey(e.id))
                    {
                        entriesDict.Add(e.id, e);
                    }
                }
            }
        }

        public SoundEntry GetSoundEntry(string id)
        {
            EnsureMap();
            return entriesDict.TryGetValue(id, out var entry) ? entry : null;
        }

        public AudioClip GetAudioClip(string id)
        {
            EnsureMap();
            return entriesDict.TryGetValue(id, out var entry) ? entry.clip : null;
        }

#if UNITY_EDITOR
        public void EditorRebuild()
        {
            entriesDict.Clear();
            EnsureMap();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    [System.Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float defaultVolume = 1f;
        [Range(0.5f, 2f)] public float defaultPitch = 1f;
        public bool spatial = false;
        public bool loop = false;
    }
}
