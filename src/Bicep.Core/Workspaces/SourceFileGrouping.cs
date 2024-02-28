// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using Bicep.Core.Diagnostics;
using Bicep.Core.Extensions;
using Bicep.Core.FileSystem;
using Bicep.Core.Navigation;
using Bicep.Core.Registry;
using Bicep.Core.Utils;

namespace Bicep.Core.Workspaces;

public record ArtifactResolutionInfo( //asdfg rename?  combine?  How is this and its members actually used?
    IArtifactReferenceSyntax DeclarationSyntax,
    BicepSourceFile SourceFile);

public record ArtifactUriResolution(
    ArtifactReference ArtifactReference,
    Uri Uri);

public record UriResolutionError(
    DiagnosticBuilder.ErrorBuilderDelegate ErrorBuilder,
    bool RequiresRestore);

public class SourceFileGrouping : IArtifactFileLookup
{
    //asdfg

    // Artifact reference syntax -> ISourceFile or Uri
    // Uri -> ISourceFile

    public SourceFileGrouping(IFileResolver fileResolver,
        Uri entryFileUri,
        // A dictionary of all source files (or rather the result of attempting to retrieve them), keyed by Uri
        ImmutableDictionary<Uri, ResultWithDiagnostic<ISourceFile>> fileResultByUri,
        // For each bicep file key, a dictionary containing all artifact URIs/references in that file, keyed by their module/artifact declaration syntax
        ImmutableDictionary<BicepSourceFile, ImmutableDictionary<IArtifactReferenceSyntax, ArtifactUriResolution>> artifactResolutionPerFileBySyntax,
        ImmutableDictionary<ISourceFile, ImmutableHashSet<ISourceFile>> sourceFileParentLookup)
    {
        FileResolver = fileResolver;
        EntryFileUri = entryFileUri;
        FileResultByUri = fileResultByUri;
        ArtifactResolutionPerFileBySyntax = artifactResolutionPerFileBySyntax;
        SourceFileParentLookup = sourceFileParentLookup;
    }

    public IFileResolver FileResolver { get; }

    public Uri EntryFileUri { get; }

    public BicepSourceFile EntryPoint
        => (FileResultByUri[EntryFileUri].TryUnwrap() as BicepSourceFile) ?? throw new InvalidOperationException($"{nameof(EntryFileUri)} is not a Bicep source file!");

    public IEnumerable<ISourceFile> SourceFiles => FileResultByUri.Values.Select(x => x.IsSuccess(out var success) ? success : null).WhereNotNull();

    public ImmutableDictionary<Uri, ResultWithDiagnostic<ISourceFile>> FileResultByUri { get; }

    public ImmutableDictionary<BicepSourceFile, ImmutableDictionary<IArtifactReferenceSyntax, ArtifactUriResolution>> ArtifactResolutionPerFileBySyntax { get; }

    public ImmutableDictionary<ISourceFile, ImmutableHashSet<ISourceFile>> SourceFileParentLookup { get; }

    public IEnumerable<ArtifactResolutionInfo> GetArtifactsToRestore(bool force = false)
    {
        foreach (var (sourceFile, artifactResults) in ArtifactResolutionPerFileBySyntax)
        {
            foreach (var (syntax, result) in artifactResults)
            {
                if (force || !result.UriResult.IsSuccess(out _, out var failure) && failure.RequiresRestore)
                {
                    yield return new(syntax, sourceFile);
                }
            }
        }
    }

    public ResultWithDiagnostic<ISourceFile> TryGetSourceFileForSyntax(IArtifactReferenceSyntax foreignTemplateReferenceSyntax)
        => TryGetFileUriForReferenceSyntax(foreignTemplateReferenceSyntax).IsSuccess(out var fileUri, out var errorBuilder) ? FileResultByUri[fileUri] : new(errorBuilder);

    public ResultWithDiagnostic<Uri> TryGetFileUriForReferenceSyntax(IArtifactReferenceSyntax foreignTemplateReferenceSyntax)
    {
        var uriResult = ArtifactResolutionPerFileBySyntax.Values.Select(d => d.TryGetValue(foreignTemplateReferenceSyntax, out var result) ? result : null).WhereNotNull().First().UriResult;
        if (!uriResult.IsSuccess(out var fileUri, out var error))
        {
            return new(error.ErrorBuilder);
        }

        return new(fileUri);
    }

    public ImmutableHashSet<ISourceFile> GetFilesDependingOn(ISourceFile sourceFile)
    {
        var filesToCheck = new Queue<ISourceFile>(new[] { sourceFile });
        var knownFiles = new HashSet<ISourceFile>();

        while (filesToCheck.TryDequeue(out var current))
        {
            knownFiles.Add(current);

            if (SourceFileParentLookup.TryGetValue(current, out var parents))
            {
                foreach (var parent in parents.Where(x => !knownFiles.Contains(x)))
                {
                    knownFiles.Add(parent);
                    filesToCheck.Enqueue(parent);
                }
            }
        }

        return knownFiles.ToImmutableHashSet();
    }
}
