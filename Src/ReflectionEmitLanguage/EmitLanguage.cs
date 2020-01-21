// ---------------------------------------------------------
// Jonathan de Halleux Reflection Emit Language for Reflector
// Copyright (c) 2007 Jonathan de Halleux. All rights reserved.
// ---------------------------------------------------------
using System;
using Reflector.CodeModel;

namespace Reflector.ReflectionEmitLanguage
{
    internal sealed class EmitLanguage : ILanguage
    {
        private readonly IServiceProvider serviceProvider;
        public EmitLanguage(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string Name
        {
            get { return "Reflection.Emit"; }
        }

        string ILanguage.FileExtension
        {
            get { return "cs"; }
        }

        ILanguageWriter ILanguage.GetWriter(IFormatter formatter, ILanguageWriterConfiguration configuration)
        {
            return new EmitLanguageWriter(this.serviceProvider,
                new EmitFormatter(formatter),
                configuration);
        }

        bool ILanguage.Translate
        {
            get { return false; }
        }
    }
}
