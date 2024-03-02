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

public class CachedValue<T>
{
    private readonly Func<T> func;
    private T? value;
    private bool hasValue;
    private readonly object locker = new();
    public CachedValue(Func<T> func) => this.func = func;

    private bool isEnabled = true;

    public bool IsEnabled
    {
        get
        {
            lock (locker)
            {
                return isEnabled;
            }
        }
        set
        {
            lock (locker)
            {
                isEnabled = value;
                if (!value)
                {
                    this.value = default;
                    hasValue = false;
                }
            }
        }
    }

    public T Value
    {
        get
        {
            lock (locker)
            {
                if (!isEnabled) return func();
                if (!hasValue)
                {
                    value = func();
                    hasValue = true;
                }

                return value!;
            }
        }
    }
}
