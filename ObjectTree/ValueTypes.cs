using System;

namespace ObjectTree
{
    [Flags]
    internal enum ValueTypes : int
    {
        Unknown = 0,
        Null = 1,
        Enum = 2,
        Boolean = 4,
        Number = 8,
        String = 16,
        Object = 32,
        Array = 64,
        Blob = 128,
        Ref = 256
    }
}
