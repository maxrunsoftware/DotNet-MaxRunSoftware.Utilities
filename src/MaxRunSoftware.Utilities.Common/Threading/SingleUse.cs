// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

[Serializable]
public sealed class SingleUse
{
    private readonly object locker;
    private volatile bool isUsed;

    public SingleUse() : this(new()) { }
    public SingleUse(object locker) => this.locker = locker.CheckNotNull(nameof(locker));

    /// <summary>
    /// Attempts to use, if it has not already been used.
    /// </summary>
    /// <returns>true if object has not already been used, otherwise false</returns>
    public bool TryUse()
    {
        if (isUsed) return false;
        lock (locker)
        {
            if (isUsed) return false;
            isUsed = true;
            return true;
        }
    }

    public bool IsUsed => isUsed;
}
