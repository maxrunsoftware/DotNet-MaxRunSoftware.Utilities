// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

using Microsoft.Extensions.DependencyInjection;

namespace MaxRunSoftware.Utilities.Common;

public static class ServiceAttributeExtensions
{
    public static IServiceCollection AddServiceAttributeServices(
        this IServiceCollection services,
        IEnumerable<(Type, ServiceAttribute)> serviceAttributeServices,
        Func<Type, ServiceAttribute, bool>? predicate = null
    )
    {
        foreach (var (serviceType, serviceAttribute) in serviceAttributeServices)
        {
            if (predicate == null || predicate(serviceType, serviceAttribute))
            {
                services.Add(serviceAttribute.ToServiceDescriptor(serviceType));
            }
        }
        
        return services;
    }

    public static IServiceCollection AddServiceAttributeServices(
        this IServiceCollection services,
        Assembly assembly,
        Func<Type, ServiceAttribute, bool>? predicate = null
    ) => AddServiceAttributeServices(
        services,
        assembly.GetTypesWithAttribute<ServiceAttribute>(),
        predicate: predicate
    );
}
