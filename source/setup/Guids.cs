// Guids.cs
// MUST match guids.h
using System;

namespace me.FastCode
{
    static class GuidList
    {
        public const string guidFastCodePkgString = "748e04d5-e239-4fbd-bfed-2c018a4c8358";
        public const string guidFastCodeCmdSetString = "07d5f27e-ddc1-409e-a69f-ab14c960398e";

        public static readonly Guid guidFastCodeCmdSet = new Guid(guidFastCodeCmdSetString);
    };
}