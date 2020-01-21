namespace Reflector.SilverlightBrowser
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Drawing;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Threading;
	using Reflector.SilverlightLoader;
	using Reflector;
	using Reflector.CodeModel;
	using Reflector.CodeModel.Memory;

	internal class Viewer : UserControl
    {
        private Uri url;
        private List<string> javaScriptFiles;
        private List<string> xamlFiles;
        private Dictionary<string, string> files = null;
		private IList<IAssembly> assemblyList = new List<IAssembly>();
		private IWindowManager windowManager;
		private IAssemblyManager assemblyManager;
		private ITranslatorManager translatorManager;
		private ILanguageManager languageManager;

		private TabControl tabControl;
		private WebBrowser webBrowser;
		private TextBox urlTextBox;
		private Button goButton;
		private ProgressBar progressBar;

		private string message;

		public Viewer(IAssemblyManager assemblyManager, ILanguageManager languageManager, ITranslatorManager translatorManager, IWindowManager windowManager)
        {
			this.assemblyManager = assemblyManager;
			this.languageManager = languageManager;
			this.translatorManager = translatorManager;
			this.windowManager = windowManager;

			this.tabControl = new TabControl();
			this.urlTextBox = new TextBox();
			this.goButton = new Button();

			this.tabControl.Dock = DockStyle.Fill;
			this.tabControl.SelectedIndex = 0;
			this.tabControl.TabIndex = 1;
			this.tabControl.TabStop = false;
			this.tabControl.Visible = false;

			this.urlTextBox.Dock = DockStyle.Fill;
			this.urlTextBox.Location = new Point(0, 0);
			this.urlTextBox.Width = 464;
			this.urlTextBox.TabIndex = 0;

			this.goButton.Dock = DockStyle.Right;
			this.goButton.Location = new Point(477, 0);
			this.goButton.Size = new Size(75, 23);
			this.goButton.TabIndex = 1;
			this.goButton.Text = "Go";
			this.goButton.FlatStyle = FlatStyle.System;
			this.goButton.UseVisualStyleBackColor = true;
			this.goButton.Click += new System.EventHandler(this.GoButton_Click);

			Control navigationBar = new Control();
			navigationBar.TabIndex = 0;
			navigationBar.TabStop = false;
			navigationBar.Dock = DockStyle.Top;
			navigationBar.Height = 21;
			navigationBar.Controls.Add(this.urlTextBox);
			navigationBar.Controls.Add(this.goButton);

			this.Controls.Add(this.tabControl);
			this.Controls.Add(navigationBar);

			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.AutoSize = true;
        }

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (this.Parent != null)
			{
				this.progressBar = new ProgressBar();
				this.windowManager.StatusBar.Controls.Add(this.progressBar);
			}
			else
			{
				this.windowManager.StatusBar.Controls.Remove(this.progressBar);
				this.progressBar = null;
			}
		}

        private void Translate()
        {
            try
            {
                this.BeginInvoke(new ThreadStart(delegate() {
						this.progressBar.Value = 0;
						this.windowManager.StatusBar.Text = "Loading... " + this.url.ToString();
					}));

				this.Invoke(new ThreadStart(delegate() {
						this.files = Sniff(this.url, this.javaScriptFiles, this.xamlFiles);
					}));

                foreach (KeyValuePair<string, string> fileContent in this.files)
                {
                    this.BeginInvoke(new ThreadStart(delegate() {
							this.progressBar.Value = 25;
							this.windowManager.StatusBar.Text = "Sniffing JavaScript";
						}));

                    TabPage page = new TabPage(fileContent.Key);

                    RichTextBox textBox = new RichTextBox();
					textBox.Text = fileContent.Value;
					textBox.WordWrap = false;
					textBox.ScrollBars = RichTextBoxScrollBars.Both;
					textBox.Dock = DockStyle.Fill;
					page.Controls.Add(textBox);

					this.BeginInvoke(new ThreadStart(delegate() {
	                        this.tabControl.TabPages.Add(page);
						}));
                }

                if (this.assemblyList.Count > 0)
                {
                    StringFormatter format = new StringFormatter();

                    ILanguageWriter languageWriter = this.languageManager.ActiveLanguage.GetWriter(format, new Config());

                    ITranslator translator = this.translatorManager.CreateDisassembler(null, null);

                    foreach (IAssembly assembly in this.assemblyList)
                    {
                        foreach (IModule mod in assembly.Modules)
                        {
                            foreach (ITypeDeclaration type in mod.Types)
                            {
                                this.BeginInvoke(new ThreadStart(delegate() {
										int changeValue = 75 / mod.Types.Count;
										if (this.progressBar.Value + changeValue < 100)
										{
											this.progressBar.Value += changeValue;
										}

										this.windowManager.StatusBar.Text = "Creating " + type.Name + this.languageManager.ActiveLanguage.FileExtension;
	                                }));

								if (type.Namespace.Length == 0 || type.Name == "<Module>" || type.Name == "<PrivateImplementationDetails>")
								{
									continue;
								}

								ITypeDeclaration typeDeclaration = translator.TranslateTypeDeclaration(type, true, true);

                                INamespace namespace2 = new Namespace();
								namespace2.Types.Add(typeDeclaration);
								namespace2.Name = typeDeclaration.Namespace;

								languageWriter.WriteNamespace(namespace2);

                                string fileName = type.Name + this.languageManager.ActiveLanguage.FileExtension;
                                TabPage page = new TabPage(fileName);

								RichTextBox textBox = new RichTextBox();
								textBox.Text = format.Value;
								textBox.WordWrap = false;
								textBox.ScrollBars = RichTextBoxScrollBars.Both;
								textBox.Dock = DockStyle.Fill;
								page.Controls.Add(textBox);

								format.Clear();

								this.BeginInvoke(new ThreadStart(delegate() {
	                                    this.tabControl.TabPages.Add(page);
									}));
                            }
                        }

						this.message = "Silverlight .NET Page Loaded";
                    }
                }

				this.BeginInvoke(new ThreadStart(delegate() {
                    this.progressBar.Value = 100;
					this.windowManager.StatusBar.Text = this.message;
	                }));
            }
            catch
            {
                this.BeginInvoke(new ThreadStart(delegate()
                {
					this.windowManager.StatusBar.Text = this.message;
                }));
            }
        }

        public TabControl Tabs
        {
            get { return this.tabControl; }
            set { this.tabControl = value; }
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            try
            {
				this.tabControl.Visible = true;

                Uri url = new Uri(this.urlTextBox.Text);

				this.xamlFiles = null;
				this.javaScriptFiles = null;
				this.files = null;
				this.assemblyList.Clear();

				this.tabControl.TabPages.Clear();

				this.webBrowser = new WebBrowser();
				this.webBrowser.Dock = DockStyle.Fill;

				TabPage tabPage = new TabPage();
				tabPage.Text = "*";
				tabPage.Controls.Add(this.webBrowser);
				this.tabControl.TabPages.Add(tabPage);

                this.webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.WebBrowser_DocumentCompleted);
                this.webBrowser.Navigate(url);
            }
            catch
            {
                this.windowManager.StatusBar.Text = "The URL entered is not a proper URI";
            }
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
			this.webBrowser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(this.WebBrowser_DocumentCompleted);

			System.Windows.Forms.HtmlDocument document = this.webBrowser.Document;

			this.url = document.Url;
			this.xamlFiles = FindXamlFiles(document);
			this.javaScriptFiles = FindJavaScriptFiles(document);

			Thread thread = new Thread(this.Translate);
			thread.Start();
        }

		private static List<string> FindXamlFiles(System.Windows.Forms.HtmlDocument doc)
        {
            List<string> xamlFiles = new List<string>();

			foreach (System.Windows.Forms.HtmlElement obj in doc.GetElementsByTagName("object"))
            {
                string type = obj.GetAttribute("type");
				switch (type)
				{
					case "application/ag-plugin":
					case "application/x-silverlight":
						foreach (System.Windows.Forms.HtmlElement param in obj.GetElementsByTagName("param"))
						{
							string name = param.GetAttribute("name");
							if (name == "source")
							{
								string value = param.GetAttribute("value");
								if (!string.IsNullOrEmpty(value))
								{
									xamlFiles.Add(value);
								}
							}
						}
						break;
				}
            }

            return xamlFiles;
        }

		private static List<string> FindJavaScriptFiles(System.Windows.Forms.HtmlDocument doc)
        {
			List<string> files = new List<string>();
			foreach (System.Windows.Forms.HtmlElement script in doc.GetElementsByTagName("script"))
            {
                string src = script.GetAttribute("src");
				if (!string.IsNullOrEmpty(src))
				{
					files.Add(src);
				}
            }
			return files;
        }

		internal Dictionary<string, string> Sniff(Uri uri, List<string> javaScriptFiles, List<string> xamlFiles)
		{
			this.message = string.Empty;

			Dictionary<string, string> files = new Dictionary<string, string>();
			try
			{
				string url = uri.ToString();
				string fileAndQuery = Path.GetFileName(uri.AbsolutePath) + uri.Query;
				Uri baseUrl = new Uri(url.Substring(0, url.Length - fileAndQuery.Length));

				foreach (string jsFile in javaScriptFiles)
				{
					string jsUrl = new Uri(baseUrl, jsFile).AbsoluteUri;
					string jsContent = DownloadText(jsUrl);
					files.Add(jsFile, jsContent);

					JavaScriptReader javaScriptReader = new JavaScriptReader(jsContent);

					foreach (string xamlFile in javaScriptReader.XamlLinks)
					{
						if (!xamlFiles.Contains(xamlFile))
						{
							xamlFiles.Add(xamlFile);
						}
					}
				}

				foreach (string xamlFile in xamlFiles)
				{
					string fileUrl = new Uri(baseUrl, xamlFile).AbsoluteUri;

					switch (Path.GetExtension(fileUrl).ToLower())
					{
						case ".xap":
							byte[] buffer = DownloadBinary(fileUrl);
							Archive archive = new Archive(new MemoryStream(buffer));
							ArchiveItem item = null;
							while ((item = archive.Read()) != null)
							{
								if (Path.GetExtension(item.Name).ToLower() == ".dll")
								{
									this.DownloadAssembly(item.Name, item.Value);
								}
							}

							break;

						case ".xaml":
							string xamlContent = DownloadText(fileUrl);
							files.Add(xamlFile, xamlContent);

							XamlReader xamlReader = new XamlReader(xamlContent);
							foreach (string assemblyName in xamlReader.AssemblyLinks)
							{
								this.DownloadAssembly(assemblyName, baseUrl);
							}
							break;
					}
				}
			}
			catch
			{
				this.message = "JavaScript Silverlight Page Loaded";
			}

			return files;
		}

		private void DownloadAssembly(string assemblyName, Uri baseUrl)
		{
			string outputPath = this.EnsureOutputPath();

			string assemblyUrl = new Uri(baseUrl, assemblyName).AbsoluteUri;
			string assemblyFile = Path.Combine(outputPath, Path.GetFileName(assemblyName));

			WebClient webClient = new WebClient();
			webClient.DownloadFile(assemblyUrl, assemblyFile);

			this.ReplaceAssembly(assemblyFile);
		}

		private void DownloadAssembly(string assemblyName, byte[] buffer)
		{
			string outputPath = this.EnsureOutputPath();
			string assemblyFile = Path.Combine(outputPath, assemblyName);

			using (BinaryWriter writer = new BinaryWriter(File.Create(assemblyFile)))
			{
				writer.Write(buffer);
			}

			this.ReplaceAssembly(assemblyFile);
		}

		private void ReplaceAssembly(string assemblyFile)
		{
			IAssembly newAssembly = this.assemblyManager.LoadFile(assemblyFile);
			if (newAssembly != null)
			{
				this.assemblyList.Add(newAssembly);
			}
		}

		private string EnsureOutputPath()
		{
			string folderPath = Environment.ExpandEnvironmentVariables(this.OutputPath);
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			return folderPath;
		}

		private static byte[] DownloadBinary(string url)
		{
			WebRequest request = HttpWebRequest.Create(new Uri(url));
			WebResponse response = request.GetResponse();
			using (Stream responseStream = response.GetResponseStream())
			{
				BinaryReader reader = new BinaryReader(responseStream);
				byte[] bytes = reader.ReadBytes((int) response.ContentLength);
				return bytes;
			}
		}


		private static string DownloadText(string url)
		{
			WebRequest request = HttpWebRequest.Create(new Uri(url));
			WebResponse response = request.GetResponse();
			using (Stream responseStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(responseStream);
				string content = reader.ReadToEnd();
				return content;
			}
		}

		private string OutputPath
		{
			get
			{
				string userProfile = Environment.ExpandEnvironmentVariables("%UserProfile%");
				string documentRoot = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

				if (documentRoot.StartsWith(userProfile))
				{
					documentRoot = "%UserProfile%" + documentRoot.Substring(userProfile.Length);
				}

				string outputPath = documentRoot;
				outputPath = Path.Combine(outputPath, "Reflector");
				outputPath = Path.Combine(outputPath, "Silverlight");
				return outputPath;
			}
		}
	}
}
