﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Options.Providers;

namespace Microsoft.CodeAnalysis.Editor.Shared.Options
{
    /// <summary>
    /// options to indicate whether a certain component in Roslyn is enabled or not
    /// </summary>
    internal sealed class EditorComponentOnOffOptions
    {
        private const string LocalRegistryPath = @"Roslyn\Internal\OnOff\Components\";
        private const string FeatureName = "EditorComponentOnOffOptions";

        public static readonly SingleValuedOption2<bool> Adornment = new(FeatureName, "Adornment", defaultValue: true,
            storageLocation: new LocalUserProfileStorageLocation(LocalRegistryPath + "Adornment"));

        public static readonly SingleValuedOption2<bool> Tagger = new(FeatureName, "Tagger", defaultValue: true,
            storageLocation: new LocalUserProfileStorageLocation(LocalRegistryPath + "Tagger"));

        public static readonly SingleValuedOption2<bool> CodeRefactorings = new(FeatureName, "CodeRefactorings", defaultValue: true,
            storageLocation: new LocalUserProfileStorageLocation(LocalRegistryPath + "Code Refactorings"));
    }
}
