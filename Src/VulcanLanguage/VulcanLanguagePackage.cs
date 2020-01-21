namespace Reflector.Application.Languages
{
  using System;
  using System.ComponentModel;

  internal class VulcanLanguagePackage : IPackage
  {
    private ILanguageManager languageManager;
    private VulcanLanguage VLanguage;

    public void Load(IServiceProvider serviceProvider)
    {
      this.VLanguage = new VulcanLanguage(true);
      //this.VLanguage.VisibilityConfiguration = (IVisibilityConfiguration) serviceProvider.GetService(typeof(IVisibilityConfiguration));
      //this.VLanguage.FormatterConfiguration = (IFormatterConfiguration) serviceProvider.GetService(typeof(IFormatterConfiguration));

      this.languageManager = (ILanguageManager) serviceProvider.GetService(typeof(ILanguageManager));

      for (int i = this.languageManager.Languages.Count - 1; i >= 0; i--)
      {
        if (this.languageManager.Languages[i].Name == "Vulcan")
        {
          this.languageManager.UnregisterLanguage(this.languageManager.Languages[i]);
        }
      }

      this.languageManager.RegisterLanguage(this.VLanguage);
    }

    public void Unload()
    {
      this.languageManager.UnregisterLanguage(this.VLanguage);
    }
  }
}
