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
    interface IWizardViewRegistrator
    {
        IWindow LoadView(IWindowManager windowManager, IWizardView view);
        void UnloadView(IWindowManager windowManager);
    }
}
