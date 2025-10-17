using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework.Profile.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Framework/Profile/ParameterBinder")]
    public class ParameterBinder : MonoBehaviour
    {
        [Tooltip("List of bingdings to apply")]
        public List<ParameterBindingData> bingdings = new();

        private readonly Dictionary<string, IProfileAccessor> _accessorCache = new();

        public void ApplyBingdings()
        {
            foreach (var bid in bingdings)
            {
                if (bid == null)
                    continue;
                if (bid.profile == null || string.IsNullOrEmpty(bid.fieldPath) || string.IsNullOrEmpty(bid.memberName))
                {
                    Debug.LogWarning($"[ParameterBinder] Skipping invalid binding on GameObject {gameObject.name}");
                    continue;
                }

                try
                {
                    var accessor = ResolveAccessor(bid.profile.GetType(), bid.fieldPath);
                    if (accessor == null)
                    {
                        Debug.LogWarning($"[ParameterBinder] Could not find field/property '{bid.fieldPath}' on profile '{bid.profile.name}'");
                        continue;
                    }
                    var sourceValue = accessor.GetValue(bid.profile);
                    if (sourceValue == null)
                    {
                        // allow null to be assigned for reference types
                        AssignValueToTarget(bid.target, bid.memberName, null);
                        continue;
                    }
                    var targetMember = FindFieldOrProperty(bid.target.GetType(), bid.memberName);
                    if (targetMember == null)
                    {
                        Debug.LogWarning($"[ParameterBinder] Target member '{bid.memberName}' not found on {bid.target.GetType().FullName}");
                        continue;
                    }
                    var targetType = GetMemberType(targetMember);
                    object converted = ConvertIfNecessary(sourceValue, targetType);
                    AssignValueToTarget(bid.target, bid.memberName, converted);
                }
                catch (Exception exp)
                {
                    Debug.LogException(exp);
                }
            }
        }

        private IProfileAccessor ResolveAccessor(Type profileType, string fieldPath)
        {
            var key = profileType.FullName + "|" + fieldPath;
            // 直接从字典中取
            if (_accessorCache.TryGetValue(key, out var acc))
                return acc;
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var member = profileType.GetProperty(fieldPath, flag) as MemberInfo ?? (MemberInfo)profileType.GetField(fieldPath, flag);
            if (member == null)
                return null;
            IProfileAccessor accessor = new ReflectionProfileAccessor(member);
            _accessorCache[key] = accessor;
            return accessor;
        }

        private static MemberInfo FindFieldOrProperty(Type t, string name)
        {
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var prop = t.GetProperty(name, flag);

            if (prop != null)
                return prop;
            var field = t.GetField(name, flag);
            return field;
        }

        private static Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo propinfo)
                return propinfo.PropertyType;
            if (member is FieldInfo fieldinfo)
                return fieldinfo.FieldType;
            throw new InvalidOperationException("Unsupported member type");
        }

        private static object ConvertIfNecessary(object source, Type targetType)
        {
            if (source == null)
                return null;
            var sourceType = source.GetType();
            if (targetType.IsAssignableFrom(sourceType))
                return source;
            try
            {
                return Convert.ChangeType(source, targetType);
            }
            catch (Exception)
            {
                if (source is float f && targetType == typeof(int))
                    return (int)f;
                if (source is int i && targetType == typeof(float))
                    return (float)i;
                throw new InvalidCastException($"Cannot convert from {sourceType} to {targetType}");
            }
        }

        private static void AssignValueToTarget(MonoBehaviour target, string memberName, object value)
        {
            var member = FindFieldOrProperty(target.GetType(), memberName);
            if (member == null) throw new MissingMemberException(target.GetType().FullName, memberName);
            if (member is PropertyInfo probInfo)
            {
                probInfo.SetValue(target, value);
            }
            else if (member is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(target, value);
            }
            else
            {
                throw new InvalidOperationException("Unsupported member type");
            }
        }

        class ReflectionProfileAccessor : IProfileAccessor
        {
            readonly MemberInfo _member;
            readonly Type _valueType;
            public ReflectionProfileAccessor(MemberInfo member)
            {
                _member = member;
                _valueType = member is PropertyInfo pi ? pi.PropertyType : ((FieldInfo)member).FieldType;
            }

            public Type ValueType => _valueType;

            public object GetValue(object profile)
            {
                if (_member is PropertyInfo pi) return pi.GetValue(profile);
                if (_member is FieldInfo fi) return fi.GetValue(profile);
                throw new InvalidOperationException("Unsupported member type");
            }
        }
    }
}
