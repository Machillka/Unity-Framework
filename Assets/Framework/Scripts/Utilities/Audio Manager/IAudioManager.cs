using UnityEngine;
namespace Framework.Utilities.AudioManager
{
    public interface IAudioManager
    {
        void PlayBGM(string clipName, float duration, bool isLoop);
        void StopBGM(float duration);
        void PauseBGM();
        void ResumeBGM();
        void SetBGMVolume(float volume, float duration);

        AudioHandle PlaySFX(string clipName, Vector3? pos = null, float? volume = null, float? pitch = null);
    }
}