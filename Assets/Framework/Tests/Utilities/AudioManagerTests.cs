using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Threading.Tasks;
using System.Reflection;
using Framework.Utilities.AudioManager;
using UnityEditor.VersionControl;
using Task = System.Threading.Tasks.Task;

namespace Tests.Utilities.AudioManagerTest
{
    public class AudioManagerPlayModeTests
    {
        AudioManager _audioManager;

        AudioCatalogSO CreateCatalogWithClip(string id, float lengthSeconds)
        {
            var catalog = ScriptableObject.CreateInstance<AudioCatalogSO>();
            var entry = new SoundEntry();
            entry.id = id;
            int frequency = 44100;
            int samples = Mathf.CeilToInt(frequency * lengthSeconds);
            var clip = AudioClip.Create(id, samples, 1, frequency, false);
            var data = new float[samples]; // silence
            clip.SetData(data, 0);
            entry.clip = clip;

            catalog.entries.Add(entry);

#if UNITY_EDITOR
            catalog.EditorRebuild();
#else
            catalog.GetAudioClip(id);
#endif
            return catalog;
        }

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("AudioManager", typeof(AudioManager));
            _audioManager = go.GetComponent<AudioManager>();

        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(_audioManager.gameObject);
        }

        [Test]
        public void AudioManager_IsNotNull()
        {
            Assert.IsNotNull(_audioManager);
        }

        [Test]
        public void AudioCatalogSO_Helper_Test()
        {
            AudioCatalogSO _audioCatalog = CreateCatalogWithClip("bmg_test", 2f);

            typeof(AudioManager).GetField("_audioCatalog", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_audioManager, _audioCatalog);

            var clip = _audioCatalog.GetAudioClip("bmg_test");
            Assert.IsNotNull(clip);
            Assert.AreEqual(clip.name, "bmg_test");
        }


        [Test]
        public async Task AudioManager_BGM_Tests()
        {
            Debug.Log("Test start");
            AudioCatalogSO _audioCatalog = CreateCatalogWithClip("bmg_test", 2f);

            typeof(AudioManager).GetField("_audioCatalog", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_audioManager, _audioCatalog);

            _audioManager.PlayBGM("bmg_test", 0.5f, false);
            await Task.Delay(1000);
            var bgmField = typeof(AudioManager).GetField("_bgm", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Log(bgmField);
            var bgmSource = (AudioSource)bgmField.GetValue(_audioManager);
            Debug.Log(bgmSource);
            Assert.IsTrue(bgmSource.isPlaying);
            await Task.Delay(1300);
            Assert.IsFalse(bgmSource.isPlaying);
        }
    }
}
