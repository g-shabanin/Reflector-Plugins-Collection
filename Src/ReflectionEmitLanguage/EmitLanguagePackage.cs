// ---------------------------------------------------------
// Jonathan de Halleux Reflection Emit Language for Reflector
// Copyright (c) 2007 Jonathan de Halleux. All rights reserved.
// ---------------------------------------------------------
using System;

namespace Reflector.ReflectionEmitLanguage
{
    public sealed class EmitLanguagePackage : IPackage
    {
        private ILanguageManager languageManager;
        private EmitLanguage language;

        public void Load(IServiceProvider serviceProvider)
        {
            this.language = new EmitLanguage(serviceProvider);

            this.languageManager = (ILanguageManager)serviceProvider.GetService(typeof(ILanguageManager));
            this.languageManager.RegisterLanguage(this.language);
        }

        public void Unload()
        {
            this.languageManager.UnregisterLanguage(this.language);
        }
    }
}
