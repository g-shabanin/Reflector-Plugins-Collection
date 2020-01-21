// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Reflector.CodeModel;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Reflector.Pex
{
    class PexPackage : 
        IPackage
    {
        ICommandBarManager commandBarManager;
        IWindowManager windowManager;
        IAssemblyBrowser assemblyBrowser;
        IConfigurationManager configurationManager;

        IWizardViewRegistrator viewRegistrator;
        IWizardDriver wizardDriver;

        IWindow wizardWindow;
        ICommandBar assemblyCommandBar;
        List<ICommandBarItem> commandBarItems;

        public void Load(IServiceProvider serviceProvider)
        {
            this.Load(
                (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager)),
                (IWindowManager)serviceProvider.GetService(typeof(IWindowManager)),
                (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser)),
                (IConfigurationManager)serviceProvider.GetService(typeof(IConfigurationManager)),
                new UIWizardViewRegistrator(),
                new ShellWizardDriver(
                    new PhysicalFileSystem(),
                    new WindowsShell()
                    ),
                new WizardViewControl()
                );
        }

        public void Load(
            ICommandBarManager commandBarManager,
            IWindowManager windowManager,
            IAssemblyBrowser assemblyBrowser,
            IConfigurationManager configurationManager,
            IWizardViewRegistrator viewRegistrator,
            IWizardDriver wizardDriver,
            IWizardView view)
        {
            this.commandBarManager = commandBarManager;
            this.windowManager = windowManager;
            this.assemblyBrowser = assemblyBrowser;
            this.configurationManager = configurationManager;
            
            this.viewRegistrator = viewRegistrator;
            this.wizardDriver = wizardDriver;

            this.commandBarItems = new List<ICommandBarItem>();

            // setting up the view
            this.View = view;
            this.View.GenerateClick += new EventHandler(View_Click);
            this.wizardWindow = this.viewRegistrator.LoadView(this.windowManager, view);

            // load config
            var configuration = this.configurationManager["Pex Wizard"];
            if (configuration.HasProperty("WizardFileName"))
                this.View.PexPath = configuration.GetProperty("WizardFileName");
            else
            {
                string wizardFileName;
                if (this.wizardDriver.TryFindDriver(out wizardFileName))
                    this.View.PexPath = wizardFileName;
            }

            // setting up the command
            this.assemblyCommandBar = this.commandBarManager.CommandBars["Browser.Assembly"];
            this.commandBarItems.Add(this.assemblyCommandBar.Items.AddSeparator());
            ICommandBarButton createButton;
            this.commandBarItems.Add(createButton = this.assemblyCommandBar.Items.AddButton("Pex Wizard", this.ShowPexWizard));

            createButton.Image = LoadIcon();

            // listening to updates
            this.assemblyBrowser.ActiveItemChanged += new EventHandler(this.ActiveItemChanged);
        }

        void View_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.View.PexPath) ||
                !File.Exists(this.View.PexPath))
            {
                MessageBox.Show("Invalid pexwizard path. We need to set the full path to pexwizard.exe", "Missing pexwizard.exe path", MessageBoxButtons.OK);
                return;
            }

            this.wizardDriver.Run(
                this.View.ActiveItem.Location,
                this.View.OutputPath,
                new WizardSettings(this.View.PexPath)
                {
                    AssertInconclusive = this.View.AssertInconclusive,
                },
                this.View
                );
        }

        public void Unload()
        {
            if (this.configurationManager != null)
            {
                var configuration = this.configurationManager["Pex Wizard"];
                configuration.SetProperty("WizardFileName", this.View.PexPath);

                this.configurationManager = null;
            }

            if (this.commandBarManager != null)
            {
                if (this.commandBarItems != null)
                {
                    foreach (var commandBarItem in this.commandBarItems)
                        this.assemblyCommandBar.Items.Remove(commandBarItem);
                    this.commandBarItems = null;
                }
                this.commandBarManager = null;
            }

            if (this.assemblyBrowser != null)
            {
                this.assemblyBrowser.ActiveItemChanged -= new EventHandler(this.ActiveItemChanged);
                this.assemblyBrowser = null;
            }

            if (this.windowManager != null)
            {
                this.windowManager.Windows.Remove("PexWizard");
                this.windowManager = null;
            }
        }

        public IWizardView View { get; private set; }

        public void ShowPexWizard(object sender, EventArgs e)
        {
            this.wizardWindow.Visible = true;
            this.AssignActiveItem();
        }

        public void ActiveItemChanged(object sender, EventArgs e)
        {
            if (this.wizardWindow.Visible)
                this.AssignActiveItem();
        }

        public void AssignActiveItem()
        {
            var activeItem = this.assemblyBrowser.ActiveItem;

            var assembly = activeItem as IAssembly;
            if (assembly != null)
            {
                AssignActiveAssembly(assembly);
                return;
            }

            var module = activeItem as IModule;
            if (module != null)
            {
                AssignActiveAssembly(module.Assembly);
                return;
            }
        }

        private void AssignActiveAssembly(IAssembly assembly)
        {
            this.View.ActiveItem = assembly;
            this.View.OutputPath = GetOutputPathFromLocation(assembly.Name, assembly.Location);
        }

        static string GetOutputPathFromLocation(string assemblyName, string location)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(directory, assemblyName + ".Tests");
        }

        public static Image LoadIcon()
        {
            using (var stream = typeof(PexPackage).Assembly.GetManifestResourceStream(
                typeof(PexPackage).Namespace + ".iconlogo.bmp"))
            {
                Bitmap bmp = (Bitmap)Bitmap.FromStream(stream);
                bmp.MakeTransparent();
                return bmp;
            }
        }
    }
}
