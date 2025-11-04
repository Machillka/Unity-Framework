using UnityEngine;
using System;

namespace Framework.Utilities.AudioManager
{
    public class AudioHandle
    {
        AudioSource _src;
        Action<AudioSource> _release;

        internal AudioHandle(AudioSource src, Action<AudioSource> release)
        {
            _src = src;
            _release = release;
        }

        public bool IsPlaying => _src != null && _src.isPlaying;

        // 关闭音效，并且调用回调函数
        public void Stop()
        {
            if (_src == null) return;
            _src.Stop();
            _release?.Invoke(_src);
            _src = null;
            _release = null;
        }

        // Internal: used by manager to return when finished
        internal void ReturnIfFinished()
        {
            if (_src == null) return;
            if (!_src.isPlaying)
            {
                _release?.Invoke(_src);
                _src = null;
                _release = null;
            }
        }
    }

}