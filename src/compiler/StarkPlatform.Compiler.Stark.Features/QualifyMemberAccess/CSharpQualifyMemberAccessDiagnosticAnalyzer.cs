﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using StarkPlatform.Compiler.Stark.Extensions;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Diagnostics;
using StarkPlatform.Compiler.QualifyMemberAccess;
using StarkPlatform.Compiler.Shared.Extensions;

namespace StarkPlatform.Compiler.Stark.QualifyMemberAccess
{
    [DiagnosticAnalyzer(LanguageNames.Stark)]
    internal sealed class CSharpQualifyMemberAccessDiagnosticAnalyzer
        : AbstractQualifyMemberAccessDiagnosticAnalyzer<SyntaxKind, ExpressionSyntax, SimpleNameSyntax>
    {
        protected override string GetLanguageName()
            => LanguageNames.Stark;

        protected override bool IsAlreadyQualifiedMemberAccess(ExpressionSyntax node)
            => node.IsKind(SyntaxKind.ThisExpression);

        // If the member is already qualified with `base.`,
        // or member is in object initialization context,
        // or member in property or field initialization, it cannot be qualified.
        protected override bool CanMemberAccessBeQualified(ISymbol containingSymbol, SyntaxNode node)
        {
            if (node.GetAncestorOrThis<AttributeSyntax>() != null)
            {
                return false;
            }

            return !(node.IsKind(SyntaxKind.BaseExpression) ||
                     node.Parent.Parent.IsKind(SyntaxKind.ObjectInitializerExpression) ||
                     IsInPropertyOrFieldInitialization(containingSymbol, node));
        }

        private bool IsInPropertyOrFieldInitialization(ISymbol containingSymbol, SyntaxNode node)
        {
            return (containingSymbol.Kind == SymbolKind.Field || containingSymbol.Kind == SymbolKind.Property) &&
                containingSymbol.DeclaringSyntaxReferences
                    .Select(declaringSyntaxReferences => declaringSyntaxReferences.GetSyntax())
                    .Any(declaringSyntax => IsInPropertyInitialization(declaringSyntax, node) || IsInFieldInitialization(declaringSyntax, node));
        }

        private bool IsInPropertyInitialization(SyntaxNode declarationSyntax, SyntaxNode node)
            => declarationSyntax.IsKind(SyntaxKind.PropertyDeclaration) && declarationSyntax.Contains(node);

        private bool IsInFieldInitialization(SyntaxNode declarationSyntax, SyntaxNode node)
            => declarationSyntax.GetAncestorsOrThis(n => n.IsKind(SyntaxKind.FieldDeclaration) && n.Contains(node)).Any();

        protected override Location GetLocation(IOperation operation) => operation.Syntax.GetLocation();
    }
}
