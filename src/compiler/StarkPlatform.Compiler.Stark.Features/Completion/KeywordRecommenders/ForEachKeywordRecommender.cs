﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using StarkPlatform.Compiler.Stark.Extensions.ContextQuery;

namespace StarkPlatform.Compiler.Stark.Completion.KeywordRecommenders
{
    internal class ForEachKeywordRecommender : AbstractSyntacticSingleKeywordRecommender
    {
        public ForEachKeywordRecommender()
            : base(SyntaxKind.ForKeyword)
        {
        }

        protected override bool IsValidContext(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
        {
            return
                context.IsStatementContext ||
                context.IsGlobalStatementContext ||
                context.IsAwaitStatementContext(position, cancellationToken);
        }
    }
}