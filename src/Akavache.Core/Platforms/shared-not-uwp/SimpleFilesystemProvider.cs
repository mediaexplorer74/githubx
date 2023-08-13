// Copyright (c) 2022 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

using Splat;

namespace Akavache;

/// <summary>
/// A wrapper around the file system.
/// </summary>
public class SimpleFilesystemProvider : IFilesystemProvider
{
    /// <inheritdoc />
    public IObservable<Stream> OpenFileForReadAsync(string path, IScheduler scheduler) => Utility.SafeOpenFileAsync(path, FileMode.Open, FileAccess.Read, FileShare.Read, scheduler);

    /// <inheritdoc />
    public IObservable<Stream> OpenFileForWriteAsync(string path, IScheduler scheduler) => Utility.SafeOpenFileAsync(path, FileMode.Create, FileAccess.Write, FileShare.None, scheduler);

    /// <inheritdoc />
    public IObservable<Unit> CreateRecursive(string path)
    {
        Utility.CreateRecursive(new(path));
        return Observable.Return(Unit.Default);
    }

    /// <inheritdoc />
    public IObservable<Unit> Delete(string path) => Observable.Start(() => File.Delete(path), BlobCache.TaskpoolScheduler);

    /// <inheritdoc />
    public string GetDefaultRoamingCacheDirectory()
    {
       return Path.Combine(GetAssemblyDirectoryName(), "BlobCache");
    }

    /// <inheritdoc />
    public string GetDefaultSecretCacheDirectory()
    {
        return Path.Combine(GetAssemblyDirectoryName(), "SecretCache");
    }

    /// <inheritdoc />
    public string GetDefaultLocalMachineCacheDirectory()
    {
        return Path.Combine(GetAssemblyDirectoryName(), "LocalBlobCache");
    }

    /// <summary>
    /// Gets the assembly directory name.
    /// </summary>
    /// <returns>The assembly directory name.</returns>
    protected static string GetAssemblyDirectoryName()
    {
        var assemblyDirectoryName =
            Path.GetDirectoryName("Location");
        return assemblyDirectoryName ?? throw new InvalidOperationException("The directory name of the assembly location is null");
    }
}