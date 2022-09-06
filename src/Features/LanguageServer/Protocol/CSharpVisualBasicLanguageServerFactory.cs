﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageServer.Handler;
using Microsoft.CommonLanguageServerProtocol.Framework;
using StreamJsonRpc;

namespace Microsoft.CodeAnalysis.LanguageServer
{
    [Export(typeof(ILanguageServerFactory)), Shared]
    internal class CSharpVisualBasicLanguageServerFactory : ILanguageServerFactory
    {
        private readonly AbstractLspServiceProvider _lspServiceProvider;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CSharpVisualBasicLanguageServerFactory(
            CSharpVisualBasicLspServiceProvider lspServiceProvider)
        {
            _lspServiceProvider = lspServiceProvider;
        }

        public async Task<AbstractLanguageServer<RequestContext>> CreateAsync(
            JsonRpc jsonRpc,
            ICapabilitiesProvider capabilitiesProvider,
            ILspServiceLogger logger)
        {
            var server = new RoslynLanguageServer(
                _lspServiceProvider,
                jsonRpc,
                capabilitiesProvider,
                logger,
                ProtocolConstants.RoslynLspLanguages,
                WellKnownLspServerKinds.CSharpVisualBasicLspServer);
            await server.InitializeAsync().ConfigureAwait(false);

            return server;
        }

        public Task<AbstractLanguageServer<RequestContext>> CreateAsync(Stream input, Stream output, ICapabilitiesProvider capabilitiesProvider, ILspServiceLogger logger)
        {
            var jsonRpc = new JsonRpc(new HeaderDelimitedMessageHandler(output, input));
            return CreateAsync(jsonRpc, capabilitiesProvider, logger);
        }
    }
}
