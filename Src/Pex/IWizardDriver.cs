// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Text;

namespace Reflector.Pex
{
    interface IWizardDriver
    {
        bool TryFindDriver(out string driverFileName);

        void Run(
            string assemblyFileName,
            string outputPath,
            WizardSettings settings,
            IWizardView view
            );
    }
}
