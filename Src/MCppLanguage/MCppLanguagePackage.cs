namespace Reflector.Application.Languages
{
	using System;
	
	internal class MCppLanguagePackage : IPackage
	{
		private ILanguageManager languageManager;
		private MCppLanguage language;
		
		public void Load(IServiceProvider serviceProvider)
		{
			this.language = new MCppLanguage(true);
			
			this.languageManager = (ILanguageManager) serviceProvider.GetService(typeof(ILanguageManager));
			
			for (int i = this.languageManager.Languages.Count - 1; i >= 0; i--)
			{
				if (this.languageManager.Languages[i].Name == "MC++")
				{
					this.languageManager.UnregisterLanguage(this.languageManager.Languages[i]);
				}
			}
			
			this.languageManager.RegisterLanguage(this.language);
		}
		
		public void Unload()
		{
			this.languageManager.UnregisterLanguage(this.language);
		}
	}
}
