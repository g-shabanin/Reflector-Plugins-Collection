// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Reflector.Pex
{
    class WindowsShell :
        IShell
    {
        public void Execute(string fileName, string parameters, Action<string> consoleSink)
        {
            var info = new ProcessStartInfo(fileName, parameters);
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            DataReceivedEventHandler sink = (sender, e) => consoleSink(e.Data);

            try
            {
                using (var process = Process.Start(info))
                {
                    try
                    {
                        process.OutputDataReceived += sink;
                        process.ErrorDataReceived += sink;
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        process.WaitForExit();
                    }
                    finally
                    {
                        process.OutputDataReceived -= sink;
                        process.ErrorDataReceived -= sink;
                    }
                }
            }
            catch (Exception ex)
            {
                consoleSink(ex.ToString());
            }
        }
    }

}
