﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.CodeActions;
using StarkPlatform.Compiler.CodeFixes;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Diagnostics;
using StarkPlatform.Compiler.Editing;
using StarkPlatform.Compiler.Shared.Extensions;
using StarkPlatform.Compiler.UseIsNullCheck;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.Stark.UseIsNullCheck
{
    [ExportCodeFixProvider(LanguageNames.Stark), Shared]
    internal class CSharpUseIsNullCheckForCastAndEqualityOperatorCodeFixProvider : SyntaxEditorBasedCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(IDEDiagnosticIds.UseIsNullCheckDiagnosticId);

        private static bool IsSupportedDiagnostic(Diagnostic diagnostic)
            => diagnostic.Properties[UseIsNullConstants.Kind] == UseIsNullConstants.CastAndEqualityKey;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            if (IsSupportedDiagnostic(diagnostic))
            {
                context.RegisterCodeFix(
                    new MyCodeAction(CSharpFeaturesResources.Use_is_null_check,
                    c => this.FixAsync(context.Document, diagnostic, c)),
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        protected override Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor, CancellationToken cancellationToken)
        {
            foreach (var diagnostic in diagnostics)
            {
                if (!IsSupportedDiagnostic(diagnostic))
                {
                    continue;
                }

                var binary = (BinaryExpressionSyntax)diagnostic.Location.FindNode(getInnermostNodeForTie: true, cancellationToken: cancellationToken);

                editor.ReplaceNode(
                    binary,
                    (current, g) => Rewrite((BinaryExpressionSyntax)current));
            }

            return Task.CompletedTask;
        }

        private static ExpressionSyntax Rewrite(BinaryExpressionSyntax binary)
        {
            var isPattern = RewriteWorker(binary);
            if (binary.IsKind(SyntaxKind.EqualsExpression))
            {
                return isPattern;
            }

            // convert:  (object)expr != null   to    !(expr is null)
            return SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                SyntaxFactory.ParenthesizedExpression(isPattern.WithoutTrivia())).WithTriviaFrom(isPattern);
        }

        private static IsPatternExpressionSyntax RewriteWorker(BinaryExpressionSyntax binary)
            => binary.Right.IsKind(SyntaxKind.NullLiteralExpression)
                ? Rewrite(binary, binary.Left, binary.Right)
                : Rewrite(binary, binary.Right, binary.Left);

        private static IsPatternExpressionSyntax Rewrite(
            BinaryExpressionSyntax binary, ExpressionSyntax expr, ExpressionSyntax nullLiteral)
        {
            var castExpr = (CastExpressionSyntax)expr;
            return SyntaxFactory.IsPatternExpression(
                castExpr.Expression.WithTriviaFrom(binary.Left),
                SyntaxFactory.Token(SyntaxKind.IsKeyword).WithTriviaFrom(binary.OperatorToken),
                SyntaxFactory.ConstantPattern(nullLiteral).WithTriviaFrom(binary.Right));
        }

        private class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(title, createChangedDocument, title)
            {
            }
        }
    }
}