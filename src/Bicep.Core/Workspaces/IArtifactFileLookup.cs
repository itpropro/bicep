// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Core.Diagnostics;
using Bicep.Core.Navigation;

namespace Bicep.Core.Workspaces;

public interface IArtifactFileLookup
{
    public ResultWithDiagnostic<ISourceFile> TryGetSourceFileForSyntax(IArtifactReferenceSyntax foreignTemplateReference);

    public ResultWithDiagnostic<Uri> TryGetFileUriForReferenceSyntax(IArtifactReferenceSyntax providerReference);
}
