﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.VisualStudio.LanguageServices.Implementation.Snippets
Imports Microsoft.VisualStudio.Text

Namespace Microsoft.VisualStudio.LanguageServices.VisualBasic.Snippets.SnippetFunctions
    Friend NotInheritable Class SnippetFunctionArgumentValue
        Inherits AbstractSnippetFunctionArgumentValue

        Public Sub New(snippetExpansionClient As SnippetExpansionClient, subjectBuffer As ITextBuffer, parameterName As String, parameterKey As SymbolKey)
            MyBase.New(snippetExpansionClient, subjectBuffer, parameterName, parameterKey)
        End Sub

        Protected Overrides ReadOnly Property FallbackDefaultLiteral As String
            Get
                Return "Nothing"
            End Get
        End Property
    End Class
End Namespace
