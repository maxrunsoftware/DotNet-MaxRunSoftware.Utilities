// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    #region NextInt

    private static volatile int nextInt;

    public static int NextInt() => Interlocked.Increment(ref nextInt);

    public static int NextInt<T>() => Interlocked.Increment(ref NextGeneric<T>.nextGeneric.nextIntInner);

    #endregion NextInt


    // ReSharper disable once ClassNeverInstantiated.Local
    private class NextGeneric<T>
    {
        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        public static readonly NextGenericInner<T> nextGeneric = new NextGenericInner<T>();

        // ReSharper disable once UnusedTypeParameter
        public class NextGenericInner<TT>
        {
            public volatile int nextIntInner;
        }
    }

}
