﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    internal sealed class SynthesizedSimpleProgramEntryPointSymbol : SourceMemberMethodSymbol
    {
        /// <summary>
        /// The corresponding <see cref="SingleTypeDeclaration"/>. 
        /// </summary>
        SingleTypeDeclaration _declaration;

        private readonly TypeSymbol _returnType;
        private WeakReference<ExecutableCodeBinder>? _weakBodyBinder;

        internal SynthesizedSimpleProgramEntryPointSymbol(SimpleProgramNamedTypeSymbol containingType, SingleTypeDeclaration declaration, DiagnosticBag diagnostics)
            : base(containingType, syntaxReferenceOpt: declaration.SyntaxReference, ImmutableArray.Create(declaration.SyntaxReference.GetLocation()), isIterator: declaration.IsIterator)
        {
            _declaration = declaration;

            bool hasAwait = declaration.HasAwaitExpressions;

            if (hasAwait)
            {
                _returnType = Binder.GetWellKnownType(containingType.DeclaringCompilation, WellKnownType.System_Threading_Tasks_Task, diagnostics, NoLocation.Singleton);
            }
            else
            {
                _returnType = Binder.GetSpecialType(containingType.DeclaringCompilation, SpecialType.System_Void, NoLocation.Singleton, diagnostics);
            }

            this.MakeFlags(
                MethodKind.Ordinary,
                DeclarationModifiers.Static | DeclarationModifiers.Private | (hasAwait ? DeclarationModifiers.Async : DeclarationModifiers.None),
                returnsVoid: !hasAwait,
                isExtensionMethod: false,
                isMetadataVirtualIgnoringModifiers: false);
        }

        public override string Name
        {
            get
            {
                return "$Main";
            }
        }

        internal override System.Reflection.MethodImplAttributes ImplementationAttributes
        {
            get { return default(System.Reflection.MethodImplAttributes); }
        }

        public override bool IsVararg
        {
            get
            {
                return false;
            }
        }

        public override ImmutableArray<TypeParameterSymbol> TypeParameters
        {
            get
            {
                return ImmutableArray<TypeParameterSymbol>.Empty;
            }
        }

        internal override int ParameterCount
        {
            get
            {
                return 0;
            }
        }

        public override ImmutableArray<ParameterSymbol> Parameters
        {
            get
            {
                return ImmutableArray<ParameterSymbol>.Empty;
            }
        }

        public override Accessibility DeclaredAccessibility
        {
            get
            {
                return Accessibility.Private;
            }
        }

        public override RefKind RefKind
        {
            get
            {
                return RefKind.None;
            }
        }

        public override TypeWithAnnotations ReturnTypeWithAnnotations
        {
            get
            {
                return TypeWithAnnotations.Create(_returnType);
            }
        }

        public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

        public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

        public override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

        public override ImmutableArray<CustomModifier> RefCustomModifiers
        {
            get
            {
                return ImmutableArray<CustomModifier>.Empty;
            }
        }

        public sealed override bool IsImplicitlyDeclared
        {
            get
            {
                return true;
            }
        }

        internal sealed override bool GenerateDebugInfo
        {
            get
            {
                return true;
            }
        }

        internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
        {
            return localPosition;
        }

        protected override void MethodChecks(DiagnosticBag diagnostics)
        {
        }

        internal override bool IsExpressionBodied => false;

        public override ImmutableArray<TypeParameterConstraintClause> GetTypeParameterConstraintClauses()
            => ImmutableArray<TypeParameterConstraintClause>.Empty;

        protected override object MethodChecksLockObject => _declaration;

        internal override CSharpSyntaxNode SyntaxNode
        {
            get
            {
                return (CSharpSyntaxNode)_declaration.SyntaxReference.SyntaxTree.GetRoot();
            }
        }

        internal CompilationUnitSyntax CompilationUnit => (CompilationUnitSyntax)SyntaxNode;

        internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, BinderFlags additionalFlags = BinderFlags.None)
        {
            // PROTOTYPE(SimplePrograms): Respect additional flags passed in by SemanticModel
            return GetBodyBinder();
        }

        private ExecutableCodeBinder CreateBodyBinder()
        {
            CSharpCompilation compilation = DeclaringCompilation;

            Binder result = new BuckStopsHereBinder(compilation);
            result = new InContainerBinder(compilation.GlobalNamespace, result, SyntaxNode, inUsing: false);
            result = new InContainerBinder(ContainingType, result);
            result = new InMethodBinder(this, result);
            return new ExecutableCodeBinder(SyntaxNode, this, result);
        }

        internal ExecutableCodeBinder GetBodyBinder()
        {
            while (true)
            {
                var previousWeakReference = _weakBodyBinder;
                if (previousWeakReference != null && previousWeakReference.TryGetTarget(out ExecutableCodeBinder? previousBinder))
                {
                    return previousBinder;
                }

                ExecutableCodeBinder newBinder = CreateBodyBinder();
                if (Interlocked.CompareExchange(ref _weakBodyBinder, new WeakReference<ExecutableCodeBinder>(newBinder), previousWeakReference) == previousWeakReference)
                {
                    return newBinder;
                }
            }
        }


        internal override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken)
        {
            if (_declaration.SyntaxReference.SyntaxTree == tree)
            {
                if (!definedWithinSpan.HasValue)
                {
                    return true;
                }
                else
                {
                    var span = definedWithinSpan.GetValueOrDefault();

                    foreach (var global in ((CompilationUnitSyntax)tree.GetRoot()).Members.OfType<GlobalStatementSyntax>())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (global.Span.IntersectsWith(span))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
