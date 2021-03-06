﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using StarkPlatform.Compiler.Stark.Symbols;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.Stark.Syntax.InternalSyntax
{
    internal class SyntaxFirstTokenReplacer : CSharpSyntaxRewriter
    {
        private readonly SyntaxToken _oldToken;
        private readonly SyntaxToken _newToken;
        private readonly int _diagnosticOffsetDelta;
        private bool _foundOldToken;

        private SyntaxFirstTokenReplacer(SyntaxToken oldToken, SyntaxToken newToken, int diagnosticOffsetDelta)
        {
            _oldToken = oldToken;
            _newToken = newToken;
            _diagnosticOffsetDelta = diagnosticOffsetDelta;
            _foundOldToken = false;
        }

        internal static TRoot Replace<TRoot>(TRoot root, SyntaxToken oldToken, SyntaxToken newToken, int diagnosticOffsetDelta)
            where TRoot : CSharpSyntaxNode
        {
            var replacer = new SyntaxFirstTokenReplacer(oldToken, newToken, diagnosticOffsetDelta);
            var newRoot = (TRoot)replacer.Visit(root);
            Debug.Assert(replacer._foundOldToken);
            return newRoot;
        }

        public override CSharpSyntaxNode Visit(CSharpSyntaxNode node)
        {
            if (node != null)
            {
                if (!_foundOldToken)
                {
                    var token = node as SyntaxToken;
                    if (token != null)
                    {
                        Debug.Assert(token == _oldToken);
                        _foundOldToken = true;
                        return _newToken; // NB: diagnostic offsets have already been updated (by SyntaxParser.AddSkippedSyntax)
                    }

                    return UpdateDiagnosticOffset(base.Visit(node), _diagnosticOffsetDelta);
                }
            }

            return node;
        }

        private static TSyntax UpdateDiagnosticOffset<TSyntax>(TSyntax node, int diagnosticOffsetDelta) where TSyntax : CSharpSyntaxNode
        {
            DiagnosticInfo[] oldDiagnostics = node.GetDiagnostics();
            if (oldDiagnostics == null || oldDiagnostics.Length == 0)
            {
                return node;
            }

            var numDiagnostics = oldDiagnostics.Length;
            DiagnosticInfo[] newDiagnostics = new DiagnosticInfo[numDiagnostics];
            for (int i = 0; i < numDiagnostics; i++)
            {
                DiagnosticInfo oldDiagnostic = oldDiagnostics[i];
                SyntaxDiagnosticInfo oldSyntaxDiagnostic = oldDiagnostic as SyntaxDiagnosticInfo;
                newDiagnostics[i] = oldSyntaxDiagnostic == null ?
                    oldDiagnostic :
                    new SyntaxDiagnosticInfo(
                        oldSyntaxDiagnostic.Offset + diagnosticOffsetDelta,
                        oldSyntaxDiagnostic.Width,
                        (ErrorCode)oldSyntaxDiagnostic.Code,
                        oldSyntaxDiagnostic.Arguments);
            }
            return node.WithDiagnosticsGreen(newDiagnostics);
        }
    }
}
