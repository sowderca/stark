﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using StarkPlatform.Compiler.EmbeddedLanguages.Common;
using StarkPlatform.Compiler.EmbeddedLanguages.VirtualChars;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.EmbeddedLanguages.RegularExpressions
{
    internal sealed class RegexTree : EmbeddedSyntaxTree<RegexKind, RegexNode, RegexCompilationUnit>
    {
        public readonly ImmutableDictionary<string, TextSpan> CaptureNamesToSpan;
        public readonly ImmutableDictionary<int, TextSpan> CaptureNumbersToSpan;

        public RegexTree(
            ImmutableArray<VirtualChar> text,
            RegexCompilationUnit root,
            ImmutableArray<EmbeddedDiagnostic> diagnostics,
            ImmutableDictionary<string, TextSpan> captureNamesToSpan,
            ImmutableDictionary<int, TextSpan> captureNumbersToSpan)
            : base(text, root, diagnostics)
        {
            CaptureNamesToSpan = captureNamesToSpan;
            CaptureNumbersToSpan = captureNumbersToSpan;
        }
    }
}
