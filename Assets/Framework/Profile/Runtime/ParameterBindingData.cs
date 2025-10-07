using System;
using UnityEngine;

namespace Framework.Profile.Runtime
{
    /// <summary>
    /// 做数值绑定的时候需要的数据参数
    /// </summary>
    public class ParameterBindingData
    {
        [Tooltip("Profile asset providing the source value.")]
        public ScriptableObject profile;
        [Tooltip("Field or property name on the profile (e.g. 'maxSpeed').")]
        public string fieldPath;
        [Tooltip("Target component that will receive the value.")]
        public MonoBehaviour target;
        public string memberName;
        [NonSerialized] public IProfileAccessor cachedAccessor;
    }
}


