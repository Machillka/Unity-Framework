using System.Collections;
using Codice.CM.SEIDInfo;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace Framework.Utilities.AudioManager
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        [SerializeField]
        private AudioCatalogSO _audioCatalog;

        private ObjectPool<AudioSource> _sourcePool;

        public AudioMixerGroup BGMGroup;
        public AudioMixerGroup SFXGroup;        // 短时播放，使用pool管理，减少资源开销

        public GameObject audioSourcePrefab;
        private Transform _sourceRootTransform;

        [Header("Pool Settings")]
        public int defaultPoolSize = 10;
        public int maxPoolSize = 20;

        AudioSource _bgm;
        Coroutine _bgmFade;
        Transform _root;

        [Header("Volumes (linear 0..1)")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        private void Awake()
        {
            _sourcePool = new ObjectPool<AudioSource>(
                createFunc: () => CreatePoolSource(),
                // 得到的时候只是激活，具体播放逻辑留给 Play 方法
                actionOnGet: s => { s.gameObject.SetActive(true); s.Stop(); s.clip = null; },
                actionOnRelease: s => { s.Stop(); s.clip = null; s.gameObject.SetActive(false); },
                actionOnDestroy: s => { if (s != null) Object.Destroy(s.gameObject); },
                collectionCheck: false,
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize
            );

            // 预热 pool
            for (int i = 0; i < defaultPoolSize; i++)
            {
                _sourcePool.Release(_sourcePool.Get());
            }

            CreateBgmSource();
            ApplyVolumes();
        }

        private void CreateBgmSource()
        {
            var go = new GameObject("BGM_Source");
            go.transform.SetParent(_root, false);
            _bgm = go.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;
            if (BGMGroup != null)
                _bgm.outputAudioMixerGroup = BGMGroup;
        }

        private AudioSource CreatePoolSource()
        {
            GameObject go;
            if (audioSourcePrefab != null)
            {
                go = Instantiate(audioSourcePrefab, _sourceRootTransform);
            }
            else
            {
                go = new GameObject("PoolAudioSource", typeof(AudioSource));
            }
            var src = go.GetComponent<AudioSource>() ?? go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            if (SFXGroup != null)
                src.outputAudioMixerGroup = SFXGroup;
            go.SetActive(false);
            return src;
        }

        // TODO: 
        public void PlayBGM(string clipName, float duration, bool isLoop)
        {
            if (_audioCatalog == null)
                return;

            var entry = _audioCatalog.GetSoundEntry(clipName);
            if (entry == null || entry.clip == null)
                return;
            if (_bgmFade != null)
            {
                StopCoroutine(_bgmFade);
            }

            _bgm.clip = entry.clip;
            _bgm.loop = isLoop;
            _bgm.Play();
            _bgmFade = StartCoroutine(FadeVolumeTo(_bgm, masterVolume * bgmVolume, duration));
        }

        public void StopBGM(float duration)
        {
            if (_bgm == null)
                return;
            if (_bgmFade != null)
                StopCoroutine(_bgmFade);
            _bgmFade = StartCoroutine(FadeOutAndStop(_bgm, duration));
        }

        public void SetBGMVolume(float volume, float duration)
        {
            if (_bgm == null)
                return;
            if (_bgmFade != null)
                StopCoroutine(_bgmFade);
            _bgmFade = StartCoroutine(FadeVolumeTo(_bgm, volume, duration));
        }

        public void PauseBGM()
        {
            if (_bgm == null)
                return;
            _bgm.Pause();
        }

        public void ResumeBGM()
        {
            if (_bgm == null)
                return;
            _bgm.UnPause();
        }

        public AudioHandle PlaySFX(string clipName, Vector3? pos = null, float? volume = null, float? pitch = null)
        {
            if (_audioCatalog == null)
                return null;

            var entry = _audioCatalog.GetSoundEntry(clipName);
            if (entry == null || entry.clip == null)
                return null;
            AudioSource src;

            try
            {
                src = _sourcePool.Get();
            }
            catch
            {
                Debug.LogWarning("AudioSource pool exhausted!");
                src = CreatePoolSource();
                src.gameObject.SetActive(true);
            }

            src.clip = entry.clip;
            src.volume = (volume ?? entry.defaultVolume) * masterVolume * sfxVolume;
            src.pitch = pitch ?? entry.defaultPitch;
            src.spatialBlend = entry.spatial ? 1f : 0f;
            src.loop = entry.loop;
            src.Play();

            var handle = new AudioHandle(src, ReleaseSource);
            if (!src.loop)
            {
                StartCoroutine(AutoReleaseWhenFinished(handle));
            }
            return handle;
        }

        private void ReleaseSource(AudioSource src)
        {
            if (_sourcePool != null && _sourcePool.CountInactive < maxPoolSize)
            {
                _sourcePool.Release(src);
            }
            else
            {
                Destroy(src.gameObject);
            }
        }

        public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); ApplyVolumes(); }
        public void SetBGMVolume(float v) { bgmVolume = Mathf.Clamp01(v); ApplyVolumes(); }
        public void SetSFXVolume(float v) { sfxVolume = Mathf.Clamp01(v); /* active sfx keep their set volume; new ones use updated value */ }

        private void ApplyVolumes()
        {
            if (_bgm != null) _bgm.volume = masterVolume * bgmVolume;
        }

        /// <summary>
        /// 在给定时间内把音量淡入或淡出到目标值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetVolume"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        IEnumerator FadeVolumeTo(AudioSource source, float targetVolume, float duration)
        {
            float t = 0f;
            float startVolume = source.volume;
            while (t < duration)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
                yield return null;
            }

            source.volume = targetVolume;
            _bgmFade = null;
        }

        /// <summary>
        /// 淡出到声音为0 并且停止播放
        /// </summary>
        /// <param name="source"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        IEnumerator FadeOutAndStop(AudioSource source, float duration)
        {
            float t = 0f;
            float startVolume = source.volume;
            while (t < duration)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
                yield return null;
            }

            source.volume = 0f;
            source.Stop();
            source.clip = null;
            _bgmFade = null;
        }

        IEnumerator AutoReleaseWhenFinished(AudioHandle handle)
        {
            while (handle.IsPlaying)
            {
                yield return null;
            }
            handle.Stop();
        }
    }
}
