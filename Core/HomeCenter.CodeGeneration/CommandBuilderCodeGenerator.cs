﻿using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.CodeGeneration
{
    public class CommandBuilderCodeGenerator : IRichCodeGenerator
    {
        private readonly AttributeData _attributeData;

        public CommandBuilderCodeGenerator(AttributeData attributeData)
        {
            _attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var proxy = new CommandBuilerGenerator().Generate(context);
            return Task.FromResult(proxy);
        }
    }
}