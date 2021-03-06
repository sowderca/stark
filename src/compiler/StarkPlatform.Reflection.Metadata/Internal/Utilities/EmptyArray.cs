// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace StarkPlatform.Reflection.Internal
{
    internal static class EmptyArray<T>
    {
        internal static readonly T[] Instance = new T[0];
    }
}
