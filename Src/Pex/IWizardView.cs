// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Text;
using Reflector.CodeModel;

namespace Reflector.Pex
{
    interface IWizardView
    {
        IAssembly ActiveItem { get; set; }
        string PexPath { get; set; }
        string OutputPath { get; set; }
        bool AssertInconclusive { get; set; }

        event EventHandler GenerateClick;
        void ExecutionStarted();
        void ExecutionFinished();
        void Progress(string message);
    }
}
