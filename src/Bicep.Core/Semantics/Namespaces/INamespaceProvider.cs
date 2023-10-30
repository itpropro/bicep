// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Bicep.Core.Features;
using Bicep.Core.Syntax;
using Bicep.Core.TypeSystem;
using Bicep.Core.Workspaces;

namespace Bicep.Core.Semantics.Namespaces;

public record TypesProviderDescriptor
{
    public TypesProviderDescriptor(
        string name,
        string? path = null,
        string? alias = null,
        string version = IResourceTypeProvider.BuiltInVersion)
    {
        Name = name;
        Alias = alias ?? name;
        Version = version;
        Path = path ?? "builtin";
    }

    public string Name { get; }

    public string Alias { get; }

    public string Path { get; }

    public string Version { get; }
}

public interface INamespaceProvider
{
    NamespaceType? TryGetNamespace(
        TypesProviderDescriptor typesProviderDescriptor,
        ResourceScope resourceScope,
        IFeatureProvider features,
        BicepSourceFileKind sourceFileKind);

    IEnumerable<string> AvailableNamespaces { get; }
}
