// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bicep.Core;
using Bicep.Core.Analyzers;
using Bicep.Core.CodeAction;
using Bicep.Core.CodeAction.Fixes;
using Bicep.Core.Diagnostics;
using Bicep.Core.Extensions;
using Bicep.Core.Parsing;
using Bicep.Core.Semantics;
using Bicep.Core.Text;
using Bicep.Core.Workspaces;
using Bicep.LanguageServer.CompilationManager;
using Bicep.LanguageServer.Completions;
using Bicep.LanguageServer.Extensions;
using Bicep.LanguageServer.Providers;
using Bicep.LanguageServer.Telemetry;
using Bicep.LanguageServer.Utils;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Bicep.LanguageServer.Handlers
{
    // Provides code actions/fixes for a range in a Bicep document
    public class BicepCodeLensHandler : CodeLensHandlerBase
    {
        private readonly IClientCapabilitiesProvider clientCapabilitiesProvider;
        private readonly ICompilationManager compilationManager;

        //asdfg     
        //private static readonly ImmutableArray<ICodeFixProvider> codeFixProviders = new ICodeFixProvider[]
        //{
        //    new MultilineObjectsAndArraysCodeFixProvider(),
        //}.ToImmutableArray<ICodeFixProvider>();

        public BicepCodeLensHandler(ICompilationManager compilationManager, IClientCapabilitiesProvider clientCapabilitiesProvider)
        {
            this.clientCapabilitiesProvider = clientCapabilitiesProvider;
            this.compilationManager = compilationManager;
        }

        public override Task<CodeLensContainer> Handle(CodeLensParams request, CancellationToken cancellationToken)
        {
            // Create a range for the entire document
            var documentRange = new Range(new Position(0, 0), new Position(int.MaxValue, int.MaxValue));

            // Create a code lens with a constant example string or markdown
            var codeLens = new CodeLens
            {
                Range = documentRange,
                Command = new Command
                {
                    Title = "The source for this module is not available.",
                    Name = "example.command",
                    Arguments = null
                }
            };

            // Return the code lens in an array
            return Task.FromResult(new CodeLensContainer(new[] { codeLens }));
        }

        public override Task<CodeLens> Handle(CodeLens request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override CodeLensRegistrationOptions CreateRegistrationOptions(CodeLensCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            DocumentSelector = DocumentSelectorFactory.CreateForBicepAndParams(),
            ResolveProvider = false
        };
    }
}
