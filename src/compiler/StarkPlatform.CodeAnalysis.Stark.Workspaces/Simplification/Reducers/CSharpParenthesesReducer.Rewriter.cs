﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using StarkPlatform.CodeAnalysis.Stark.Syntax;
using StarkPlatform.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace StarkPlatform.CodeAnalysis.Stark.Simplification
{
    internal partial class CSharpParenthesesReducer
    {
        private class Rewriter : AbstractReductionRewriter
        {
            public Rewriter(ObjectPool<IReductionRewriter> pool)
                : base(pool)
            {
            }

            public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
            {
                return SimplifyExpression(
                    node,
                    newNode: base.VisitParenthesizedExpression(node),
                    simplifier: s_simplifyParentheses);
            }
        }
    }
}