﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using StarkPlatform.CodeAnalysis.Host;
using StarkPlatform.CodeAnalysis.Host.Mef;

namespace StarkPlatform.CodeAnalysis.CodeRefactorings
{
    [ExportWorkspaceServiceFactory(typeof(ICodeRefactoringHelpersService), ServiceLayer.Default), Shared]
    internal class ServicesLayerCodeActionHelpersService : IWorkspaceServiceFactory
    {
        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            return new CodeActionHelpersService();
        }

        private class CodeActionHelpersService : ICodeRefactoringHelpersService
        {
            public bool ActiveInlineRenameSession => false;
        }
    }
}