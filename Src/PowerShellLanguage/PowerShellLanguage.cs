namespace Reflector.PowerShellLanguage
{
	using System;
	using System.Text;
	using Reflector.CodeModel;

	internal sealed class PowerShellLanguage : ILanguage
	{
		public string FileExtension
		{
			get 
			{ 
				return "ps1"; 
			}
		}

		public ILanguageWriter GetWriter(IFormatter formatter, ILanguageWriterConfiguration configuration)
		{
			return new PowerShellLanguageWriter(formatter, configuration);
		}

		public string Name
		{
			get 
			{ 
				return "PowerShell"; 
			}
		}

		public bool Translate
		{
			get 
			{ 
				return true; 
			}
		}
	}
}
