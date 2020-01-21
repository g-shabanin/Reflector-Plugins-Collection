// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace Reflector.Pex
{
    class ShellWizardDriver :
        IWizardDriver
    {
        readonly IFileSystem fileSystem;
        readonly IShell shell;

        public ShellWizardDriver(
            IFileSystem fileSystem,
            IShell shell)
        {
            this.fileSystem = fileSystem;
            this.shell = shell;
        }

        public void Run(
            string assemblyFileName, 
            string outputPath, 
            WizardSettings settings,
            IWizardView view)
        {
            new Thread(delegate(object o)
            {
                try
                {
                    view.ExecutionStarted();

                    if (this.fileSystem.Exists(settings.WizardFileName))
                    {
                        var arguments =
                            String.Format("\"{0}\" /o:\"{1}\" /nocsc /c /op",
                                Environment.ExpandEnvironmentVariables(assemblyFileName),
                                outputPath
                                );
                        if (settings.AssertInconclusive)
                            arguments += " /ic";

                        this.shell.Execute(
                            settings.WizardFileName,
                            arguments,
                            view.Progress);
                    }
                }
                finally
                {
                    view.ExecutionFinished();
                }
            }).Start(null);
        }

        const string RootKey = @"SOFTWARE\Microsoft\Pex\CurrentVersion";
        const string InstallPathValue = "InstallPath";

        public bool TryFindDriver(out string driverFileName)
        {
            // look up registry
            using (RegistryKey rg = Registry.LocalMachine.OpenSubKey(RootKey))
            {
                if (rg != null)
                {
                    var installDirectory = (string)rg.GetValue(InstallPathValue);
                    driverFileName = Path.Combine(Path.Combine(installDirectory, "bin"), "pexwizard.exe");
                    return File.Exists(driverFileName);
                }
            }

            driverFileName = null;
            return false;
        }
    }
}
