﻿using System;
using System.Reflection;
using System.Runtime.Loader;

namespace GenerateAspNetCoreClient
{
    internal class CustomLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver dependencyResolver;
        private readonly Assembly sharedAssemply;

        public CustomLoadContext(string assemblyPath, Assembly sharedAssembly)
        {
            dependencyResolver = new AssemblyDependencyResolver(assemblyPath);
            sharedAssemply = sharedAssembly;
        }

        public Assembly Load(AssemblyName assemblyName, bool fallbackToDefault)
        {
            if (assemblyName.FullName == sharedAssemply.FullName)
                return sharedAssemply;

            var path = dependencyResolver.ResolveAssemblyToPath(assemblyName);

            if (path == null && fallbackToDefault)
            {
                var defaultLoaded = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);

                if (defaultLoaded != null)
                    path = defaultLoaded.Location;
            }

            return path != null ? LoadFromAssemblyPath(path) : null;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Load(assemblyName, true);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var resolvedPath = dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            return resolvedPath == null
                ? IntPtr.Zero
                : LoadUnmanagedDllFromPath(resolvedPath);
        }
    }
}
