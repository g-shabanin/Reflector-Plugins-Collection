namespace Reflector.SilverlightLoader
{
	using System;
	using System.Diagnostics;
	using System.Collections;
	using System.Drawing;
	using System.IO;
	using System.Globalization;
	using System.Net;
	using System.Runtime.InteropServices;
	using System.Security;
	using System.Security.Permissions;
	using System.Windows.Forms;
	using Reflector;
	using Reflector.CodeModel;

	internal class SilverlightLoaderDialog : Form
	{
		private TextBox urlTextBox;
		private Button addButton;
		private Button loadButton;
		private Button cancelButton;
		private BrowserView browser;

		public SilverlightLoaderDialog(IConfigurationManager configurationManager, IAssemblyManager assemblyManager)
		{
			this.Text = "Open Silverlight Page";
			this.Icon = null;
			this.Font = new Font("Tahoma", 8.25f);
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.ControlBox = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.StartPosition = FormStartPosition.CenterParent;
			this.AutoScale = true;
			this.AutoScaleBaseSize = new Size(5, 14);

			this.SizeGripStyle = SizeGripStyle.Hide;
			this.MinimumSize = new Size(300, 200);

			this.urlTextBox = new TextBox();
			this.urlTextBox.Location = new Point(12, 12);
			this.urlTextBox.Width = 200;
			this.urlTextBox.TextChanged += new EventHandler(this.UrlTextBox_TextChanged);
			this.Controls.Add(this.urlTextBox);

			this.addButton = new Button();
			this.addButton.FlatStyle = FlatStyle.System;
			this.addButton.Top = 11;
			this.addButton.Size = new Size(75, 23);
			this.addButton.Text = "&Add";
			this.addButton.Enabled = false;
			this.addButton.Click += new EventHandler(this.AddButton_Click);
			this.Controls.Add(this.addButton);

			this.browser = new BrowserView(configurationManager, assemblyManager);
			this.browser.Location = new Point(12, 48);
			this.browser.Size = new Size(280, 200);
			this.browser.AfterSelect += new TreeViewEventHandler(this.Browser_AfterSelect);
			this.Controls.Add(this.browser);

			this.loadButton = new Button();
			this.loadButton.FlatStyle = FlatStyle.System;
			this.loadButton.Size = new Size(75, 23);
			this.loadButton.Text = "&Load";
			this.loadButton.Click += new EventHandler(this.LoadButton_Click);
			this.Controls.Add(this.loadButton);
			this.AcceptButton = this.loadButton;

			this.cancelButton = new Button();
			this.cancelButton.FlatStyle = FlatStyle.System;
			this.cancelButton.Size = new Size(75, 23);
			this.cancelButton.Text = "&Close";
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.Controls.Add(this.cancelButton);
			this.CancelButton = this.cancelButton;

			this.ClientSize = new Size(400, 300);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			this.urlTextBox.Width = this.ClientSize.Width - 12 - 12 - this.addButton.Width - 6;
			this.addButton.Left = this.urlTextBox.Right + 6;
			this.cancelButton.Top = this.ClientSize.Height - 10 - this.cancelButton.Height;
			this.cancelButton.Left = this.ClientSize.Width - 10 - this.cancelButton.Width;
			this.loadButton.Top = this.cancelButton.Top;
			this.loadButton.Left = this.cancelButton.Left - 8 - this.loadButton.Width;
			this.browser.Top = this.urlTextBox.Bottom + 8;
			this.browser.Width = this.addButton.Right - this.browser.Left;
			this.browser.Height = this.cancelButton.Top - this.browser.Top - 10;
		}

		protected override CreateParams CreateParams
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 1; // this.ShowIcon = false;
				return createParams;
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			this.OnSizeChanged(EventArgs.Empty);
			base.OnVisibleChanged(EventArgs.Empty);

			try
			{
				NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETICON, 0, IntPtr.Zero);
				NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETICON, 1, IntPtr.Zero);
			}
			catch
			{
			}
		}

		private void AddButton_Click(object sender, EventArgs e)
		{
			if ((this.urlTextBox.Text != null) && (this.urlTextBox.Text.Length > 0))
			{
				this.browser.AddItem(this.urlTextBox.Text);
				this.browser.Enabled = true;
			}
		}

		private void UrlTextBox_TextChanged(object sender, EventArgs e)
		{
			this.addButton.Enabled = ((this.urlTextBox.Text != null) && (this.urlTextBox.Text.Length > 0));
		}

		private void Browser_AfterSelect(object sender, TreeViewEventArgs e)
		{
			this.loadButton.Enabled = (this.browser.SelectedNode is BrowserItem);
			this.loadButton.Text = (this.browser.SelectedNode is AssemblyItem) ? "&Load" : "&Open";
		}

		private void LoadButton_Click(object sender, EventArgs e)
		{
			BrowserItem item = this.browser.SelectedNode as BrowserItem;
			if (item != null)
			{
				item.Activate();
			}
		}

		private class BrowserView : TreeView
		{
			private IConfigurationManager configurationManager;
			private IAssemblyManager assemblyManager;

			public BrowserView(IConfigurationManager configurationManager, IAssemblyManager assemblyManager)
			{
				this.configurationManager = configurationManager;
				this.assemblyManager = assemblyManager;

				this.ShowLines = false;
				this.HotTracking = true;
				this.HideSelection = false;

				this.ImageList = new ImageList();
				this.ImageList.ImageSize = new Size(16, 16);
				this.ImageList.Images.AddStrip(new Bitmap(this.GetType().Assembly.GetManifestResourceStream("Reflector.SilverlightLoader.Icon.png")));
				this.ImageList.ColorDepth = ColorDepth.Depth32Bit;
				this.ImageList.TransparentColor = Color.FromArgb(255, 0, 128, 0);

				this.LoadConfiguration();
			}

			public void AddItem(string url)
			{
				HtmlItem item = new HtmlItem(string.Empty, url);
				AddItem(this.Nodes, item);
				this.SaveConfiguration();
			}

			public void RemoveItem(string url)
			{
				RemoveItem(this.Nodes, url);
				this.SaveConfiguration();
			}

			protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
			{
				BrowserItem item = this.SelectedNode as BrowserItem;
				if (item != null)
				{
					switch (keyData)
					{
						case Keys.Control | Keys.C:
							Clipboard.SetDataObject(item.AbsoluteUrl, true);
							return true;

						case Keys.Space:
							item.Activate();
							return true;

						case Keys.Delete:
							if (item.Parent == null)
							{
								this.RemoveItem(item.AbsoluteUrl);
							}
							return true;
					}
				}
				
				return base.ProcessCmdKey(ref msg, keyData);
			}

			protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
			{
				ContentItem item = e.Node as ContentItem;
				if (item != null)
				{
					item.Populate();
				}

				base.OnBeforeExpand(e);
			}

			internal static void AddItem(TreeNodeCollection parent, BrowserItem value)
			{
				foreach (TreeNode node in parent)
				{
					BrowserItem item = node as BrowserItem;
					if (item != null)
					{
						if ((value.AbsoluteUrl != null) && (value.AbsoluteUrl.Length != 0))
						{
							if ((item.AbsoluteUrl != null) && (item.AbsoluteUrl.Length != 0))
							{
								if (new Uri(value.AbsoluteUrl).Equals(new Uri(item.AbsoluteUrl)))
								{
									return;
								}
							}
						}
					}
				}

				parent.Add(value);
			}

			internal static void RemoveItem(TreeNodeCollection parent, string url)
			{
				for (int i = parent.Count - 1; i >= 0; i--)
				{
					BrowserItem item = parent[i] as BrowserItem;
					if (item != null)
					{
						if (new Uri(url).Equals(new Uri(item.AbsoluteUrl)))
						{
							parent.RemoveAt(i);
						}
					}
				}
			}

			public void LoadAssembly(string location)
			{
				this.assemblyManager.LoadFile(location);
			}

			private void LoadConfiguration()
			{
				this.Nodes.Clear();

				IConfiguration configuration = this.configurationManager["Reflector.SilverlightLoader"];
				if (configuration.HasProperty("0"))
				{
					int index = 0;
					while (configuration.HasProperty(index.ToString(CultureInfo.InvariantCulture)))
					{
						string url = configuration.GetProperty(index.ToString(CultureInfo.InvariantCulture));
						AddItem(this.Nodes, new HtmlItem(string.Empty, url));
						index++;
					}
				}
			}

			private void SaveConfiguration()
			{
				IConfiguration configuration = this.configurationManager["Reflector.SilverlightLoader"];
				configuration.Clear();
				for (int i = 0; i < this.Nodes.Count; i++)
				{
					BrowserItem item = this.Nodes[i] as BrowserItem;
					if (item != null)
					{
						string url = item.AbsoluteUrl;
						configuration.SetProperty(i.ToString(CultureInfo.InvariantCulture), url);
					}
				}
			}
		}

		private class BrowserItem : TreeNode
		{
			private string baseUrl;
			private string relativeUrl;
			private string absoluteUrl;
			private int image;

			protected BrowserItem(string baseUrl, string relativeUrl)
			{
				this.baseUrl = baseUrl;
				this.relativeUrl = relativeUrl;
	
				Uri uri = (baseUrl.Length == 0) ? new Uri(relativeUrl) : new Uri(new Uri(baseUrl), relativeUrl);
				this.absoluteUrl = uri.AbsoluteUri;

				this.Text = this.relativeUrl;
			}
 
			protected BrowserItem(string name)
			{
				this.Text = name;
			}

			public int Image
			{
				get
				{
					return this.image;
				}

				set
				{
					this.image = value;
					this.ImageIndex = this.SelectedImageIndex = this.image;
				}
			}

			public string AbsoluteUrl
			{
				get
				{
					return this.absoluteUrl;
				}
			}

			protected string BaseUrl
			{
				get
				{
					return this.baseUrl;
				}
			}

			protected void AddItem(BrowserItem value)
			{
				BrowserView.AddItem(this.Nodes, value);
			}

			public virtual void Activate()
			{
				if ((this.AbsoluteUrl != null) && (this.AbsoluteUrl.Length != 0))
				{
					try
					{
						Process.Start(this.AbsoluteUrl);
					}
					catch (Exception exception)
					{
						MessageBox.Show(exception.Message, "Silverlight Loader Add-In", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}

			protected WebResponse GetWebResponse()
			{
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create(this.AbsoluteUrl);
				request.Headers.Add("Cache-Control", "no-cache, no-store, max-age=0");
				request.Credentials = CredentialCache.DefaultCredentials;
				request.UserAgent = "Reflector.SilverlightLoader";

				WebResponse response = request.GetResponse();
				if (response.ResponseUri != null)
				{
					this.absoluteUrl = response.ResponseUri.AbsoluteUri;
				}

				int contentLength = (int) response.ContentLength;

				byte[] buffer = new byte[contentLength];
				using (Stream stream = response.GetResponseStream())
				{
					int bytesRead = 0;
					int position = 0;
					do
					{
						bytesRead = stream.Read(buffer, position, contentLength);
						contentLength -= bytesRead;
						position += bytesRead;
					}
					while (bytesRead != 0);

					if (contentLength != 0)
					{
						throw new WebException();
					}
				}

				return new MemoryWebResponse(buffer, response.ContentType);
			}
		}

		private class ContentItem : BrowserItem
		{
			private bool populated;
	
			protected ContentItem(string baseUrl, string relativeUrl) : base(baseUrl, relativeUrl)
			{
				this.populated = false;
				this.Nodes.Add(new TreeNode());
			}

			public void Populate()
			{
				if (!this.populated)
				{
					this.Nodes.Clear();

					Cursor oldCursor = Cursor.Current;
					try
					{
						Cursor.Current = Cursors.WaitCursor;

						this.ImageIndex = this.SelectedImageIndex = BrowserImage.WaitCursor;
						System.Windows.Forms.Application.DoEvents();

						WebResponse response = this.GetWebResponse();
						using (Stream responseStream = response.GetResponseStream())
						{
							this.Parse(responseStream, response.ContentType);
						}

						this.Image = this.Image;

						System.Windows.Forms.Application.DoEvents();

						this.populated = true;
					}
					catch (Exception exception)
					{
						this.Image = this.Image;

						ErrorNode errorNode = new ErrorNode(exception.Message);
						this.Nodes.Add(errorNode);

						System.Windows.Forms.Application.DoEvents();
					}
					finally
					{
						Cursor.Current = oldCursor;
					}
				}
			}

			protected virtual void Parse(Stream stream, string contentType)
			{
			}
		}

		private class HtmlItem : ContentItem
		{
			public HtmlItem(string baseUrl, string relativeUrl) : base(baseUrl, relativeUrl)
			{
				this.Image = BrowserImage.Html;
			}

			protected override void Parse(Stream stream, string contentType)
			{
				if (contentType.StartsWith("text/html") || (contentType == "application/octet-stream"))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						HtmlReader htmlReader = new HtmlReader(reader.ReadToEnd());

						foreach (string url in htmlReader.XamlLinks)
						{
							switch (Path.GetExtension(url).ToLower())
							{
								case ".xap":
									this.AddItem(new XapItem(this.AbsoluteUrl, url));
									break;

								default:
									this.AddItem(new XamlItem(this.AbsoluteUrl, url));
									break;
							}
						}

						foreach (string url in htmlReader.JavaScriptLinks)
						{
							JavaScriptItem item = new JavaScriptItem(this.AbsoluteUrl, url);
							this.AddItem(item);
						}

						foreach (string url in htmlReader.HtmlLinks)
						{
							HtmlItem item = new HtmlItem(this.AbsoluteUrl, url);
							item.ForeColor = Color.Gray;
							this.AddItem(item);
						}
					}
				}
			}
		}

		private class JavaScriptItem : ContentItem
		{
			public JavaScriptItem(string baseUrl, string relativeUrl) : base(baseUrl, relativeUrl)
			{
				this.Image = BrowserImage.JavaScript;

				string fileName = Path.GetFileName(this.AbsoluteUrl);
				if (fileName == "Silverlight.js")
				{
					this.ForeColor = Color.Gray;
				}
			}

			protected override void Parse(Stream stream, string contentType)
			{
				switch (contentType)
				{
					case "application/x-javascript":
					case "application/octet-stream":

						using (StreamReader reader = new StreamReader(stream))
						{
							JavaScriptReader javaScriptReader = new JavaScriptReader(reader.ReadToEnd());

							foreach (string url in javaScriptReader.XamlLinks)
							{
								switch (Path.GetExtension(url).ToLower())
								{
									case ".xap":
										this.AddItem(new XapItem(this.BaseUrl, url));
										break;

									default:
										this.AddItem(new XamlItem(this.BaseUrl, url));
										break;
								}
							}
						}

						break;
				}
			}
		}

		private class XamlItem : ContentItem
		{
			public XamlItem(string baseUrl, string relativeUrl) : base(baseUrl, relativeUrl)
			{
				this.Image = BrowserImage.Xaml;
			}

			protected override void Parse(Stream stream, string contentType)
			{
				if ((contentType.StartsWith("text/plain")) || (contentType.StartsWith("text/xml")) || (contentType == "application/xaml+xml") || (contentType == "application/octet-stream"))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						XamlReader xamlReader = new XamlReader(reader.ReadToEnd());

						foreach (string url in xamlReader.AssemblyLinks)
						{
							AssemblyItem item = new AssemblyItem(this.AbsoluteUrl, url);
							this.AddItem(item);
						}

						foreach (string url in xamlReader.SourceLinks)
						{
							FileItem item = new FileItem(this.AbsoluteUrl, url);
							this.AddItem(item);
						}
					}
				}
			}
		}

		private class XapItem : ContentItem
		{
			public XapItem(string baseUrl, string relativeUrl) : base(baseUrl, relativeUrl)
			{
				this.Image = BrowserImage.Xap;
			}

			protected override void Parse(Stream stream, string contentType)
			{
				if (contentType == "application/x-silverlight-app")
				{
					stream.Position = 0;

					Archive archive = new Archive(stream);
					ArchiveItem item = null;
					while ((item = archive.Read()) != null)
					{
						if (Path.GetExtension(item.Name).ToLower() == ".dll")
						{
							this.AddItem(new AssemblyItem(this.AbsoluteUrl + "/", item.Name, item.Value));
						}
					}
				}
			}
		}

		private class AssemblyItem : BrowserItem
		{
			private byte[] assembly;

			public AssemblyItem(string baseUrl, string relativeUrl) : base(baseUrl, relativeUrl)
			{
				this.Image = BrowserImage.Assembly;
				this.assembly = null;
			}

			public AssemblyItem(string baseUrl, string relativeUrl, byte[] assembly) : base(baseUrl, relativeUrl)
			{
				this.Image = BrowserImage.Assembly;
				this.assembly = assembly;
			}

			public override void Activate()
			{
				Cursor oldCursor = Cursor.Current;
				try
				{
					Cursor.Current = Cursors.WaitCursor;

					byte[] buffer = null;

					if (this.assembly != null)
					{
						buffer = this.assembly;
					}
					else
					{
						WebResponse response = this.GetWebResponse();
						using (Stream stream = response.GetResponseStream())
						{
							int length = (int) response.ContentLength;
							buffer = new byte[length];
							stream.Read(buffer, 0, length);
						}
					}

					string location = this.CopyToLocalPath(this.AbsoluteUrl, this.OutputPath, buffer);
					if (location.Length != 0)
					{
						BrowserView browser = (BrowserView)this.TreeView;
						browser.LoadAssembly(location);
					}
				}
				catch
				{
					this.ImageIndex = this.SelectedImageIndex = BrowserImage.Error;
					System.Windows.Forms.Application.DoEvents();
				}
				finally
				{
					Cursor.Current = oldCursor;
				}
			}

			private string CopyToLocalPath(string url, string cachePath, byte[] buffer)
			{
				/* 
				Uri uri = new Uri(url);
				string localPath = Path.Combine(uri.Scheme, uri.Host);

				foreach (string segment in uri.Segments)
				{
					string part = segment.Trim(new char[] { '/', '\\' });
					if (part.Length != 0)
					{
						localPath = Path.Combine(localPath, part);
					}
				}

				cachePath = Path.Combine(cachePath, Path.GetDirectoryName(localPath));
				*/

				if (!Directory.Exists(Environment.ExpandEnvironmentVariables(cachePath)))
				{
					Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(cachePath));
				}

				string location = Path.Combine(cachePath, Path.GetFileName(url));
				if (File.Exists(Environment.ExpandEnvironmentVariables(location)))
				{
					if (MessageBox.Show(string.Format("File '{0}' exists. Do you want to overwrite?", location), "Silverlight Loader Add-In", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
					{
						return string.Empty;
					}
				}

				using (Stream outputStream = File.Create(Environment.ExpandEnvironmentVariables(location)))
				{
					outputStream.Write(buffer, 0, buffer.Length);
				}

				return location;
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

		private class FileItem : BrowserItem
		{
			public FileItem(string baseUrl, string relativeUrl) : base(baseUrl, relativeUrl)
			{
				this.Image = BrowserImage.File;
			}
		}

		private class ErrorNode : BrowserItem
		{
			public ErrorNode(string message) : base(message)
			{
				this.Image = BrowserImage.Error;
			}
		}

		private class BrowserImage
		{
			public const int Error = 1;
			public const int WaitCursor = 2;
			public const int Html = 4;
			public const int JavaScript = 5;
			public const int Xaml = 6;
			public const int Assembly = 7;
			public const int File = 8;
			public const int Xap = 9;
		}

		private sealed class MemoryWebResponse : WebResponse
		{
			private byte[] buffer;
			private string contentType;

			public MemoryWebResponse(byte[] buffer, string contentType)
			{
				this.buffer = buffer;
				this.contentType = contentType;
			}

			public override long ContentLength
			{
				get
				{
					return this.buffer.Length;
				}
			}

			public override string ContentType
			{
				get
				{
					return this.contentType;
				}
			}

			public override Stream GetResponseStream()
			{
				return new MemoryStream(this.buffer);
			}
		}

		private sealed class NativeMethods
		{
			private NativeMethods()
			{
			}

			public const int WM_SETICON = 0x80;

			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
		}
	}
}
