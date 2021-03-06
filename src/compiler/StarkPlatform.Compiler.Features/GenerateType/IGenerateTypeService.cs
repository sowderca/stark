﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.CodeActions;
using StarkPlatform.Compiler.Host;

namespace StarkPlatform.Compiler.GenerateType
{
    internal interface IGenerateTypeService : ILanguageService
    {
        Task<ImmutableArray<CodeAction>> GenerateTypeAsync(Document document, SyntaxNode node, CancellationToken cancellationToken);
        Task<Tuple<INamespaceSymbol, INamespaceOrTypeSymbol, Location>> GetOrGenerateEnclosingNamespaceSymbolAsync(INamedTypeSymbol namedTypeSymbol, string[] containers, Document selectedDocument, SyntaxNode selectedDocumentRoot, CancellationToken cancellationToken);
        string GetRootNamespace(CompilationOptions options);
    }
}
