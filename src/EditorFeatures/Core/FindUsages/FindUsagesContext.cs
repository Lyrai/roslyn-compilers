﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.FindUsages
{
    internal abstract class FindUsagesContext : IFindUsagesContext
    {
        private readonly IGlobalOptionService _globalOptions;

        public IStreamingProgressTracker ProgressTracker { get; }

        protected FindUsagesContext(IGlobalOptionService globalOptions)
        {
            ProgressTracker = new StreamingProgressTracker(ReportProgressAsync);
            _globalOptions = globalOptions;
        }

        public ValueTask<FindUsagesOptions> GetOptionsAsync(string language, CancellationToken cancellationToken)
            => ValueTaskFactory.FromResult(_globalOptions.GetFindUsagesOptions(language));

        public virtual ValueTask ReportMessageAsync(string message, CancellationToken cancellationToken) => default;

        public virtual ValueTask ReportInformationalMessageAsync(string message, CancellationToken cancellationToken) => default;

        public virtual ValueTask SetSearchTitleAsync(string title, CancellationToken cancellationToken) => default;

        public virtual ValueTask OnCompletedAsync(CancellationToken cancellationToken) => default;

        public virtual ValueTask OnDefinitionFoundAsync(DefinitionItem definition, CancellationToken cancellationToken) => default;

        public virtual ValueTask OnReferenceFoundAsync(SourceReferenceItem reference, CancellationToken cancellationToken) => default;

        protected virtual ValueTask ReportProgressAsync(int current, int maximum, CancellationToken cancellationToken) => default;
    }
}
