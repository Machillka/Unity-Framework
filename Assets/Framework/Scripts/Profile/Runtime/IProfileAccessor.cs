using System;

namespace Framework.Profile.Runtime
{
    // 辅助对于 profile 中的数值绑定
    public interface IProfileAccessor
    {
        Type ValueType { get; }
        object GetValue(object profile);
    }
}