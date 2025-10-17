using UnityEngine;

namespace Framework.Profile.Scriptable
{
    /// <summary>
    /// 配置文件基类
    /// </summary>
    public abstract class BaseProfileSO : ScriptableObject
    {
        [Header("Profile Meta")]
        public string profileId;
        public string version = "0.1.0";
        public string description;
        public Texture2D preview;

        public virtual string Validate()
        {
            if (string.IsNullOrEmpty(profileId))
                return "profileID is required";
            return null;
        }
    }
}

