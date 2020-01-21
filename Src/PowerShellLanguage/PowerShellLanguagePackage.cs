namespace Reflector.PowerShellLanguage
{
	using System;
	using Reflector;

	public class PowerShellLanguagePackage : IPackage
	{
		private ILanguageManager languageManager;
		private PowerShellLanguage language;

		public void Load(IServiceProvider serviceProvider)
		{
			this.language = new PowerShellLanguage();

			this.languageManager = (ILanguageManager)serviceProvider.GetService(typeof(ILanguageManager));
			this.languageManager.RegisterLanguage(this.language);
		}

		public void Unload()
		{
			this.languageManager.UnregisterLanguage(this.language);
		}
	}
}
