﻿// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities;

public delegate object TypeConverter(object inputObject, Type outputType);

public static class TypeConverterExtensions
{
    public static TypeConverter AsTypeConverter<TInput, TOutput>(this Converter<TInput, TOutput> converter) => (inputObject, _) => converter((TInput)inputObject);

    public static Converter<TInput, TOutput> AsConverter<TInput, TOutput>(this TypeConverter typeConverter) => inputObject => (TOutput)typeConverter(inputObject, typeof(TOutput));
}

// TODO: Do something with this

public interface ITypeConverter
{
    bool TryConvert(object input, Type outputType, out object output);
}

public abstract class TypeConverterBase : ITypeConverter
{
    public abstract bool TryConvert(object input, Type outputType, out object output);
}

public class TypeConverterString : TypeConverterBase
{
    public override bool TryConvert(object input, Type outputType, out object output) => throw new NotImplementedException();
}
