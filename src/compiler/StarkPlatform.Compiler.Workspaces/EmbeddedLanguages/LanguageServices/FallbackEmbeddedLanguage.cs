﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using StarkPlatform.Compiler.Classification.Classifiers;

namespace StarkPlatform.Compiler.EmbeddedLanguages.LanguageServices
{
    /// <summary>
    /// A 'fallback' embedded language that can classify normal escape sequences in 
    /// C# or VB strings if no other embedded languages produce results.
    /// </summary>
    internal partial class FallbackEmbeddedLanguage : IEmbeddedLanguage
    {
        public FallbackEmbeddedLanguage(EmbeddedLanguageInfo info)
        {
            Classifier = new FallbackSyntaxClassifier(info);
        }

        public ISyntaxClassifier Classifier { get; }
    }
}
