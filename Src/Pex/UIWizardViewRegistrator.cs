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
    class UIWizardViewRegistrator :
        IWizardViewRegistrator
    {
        const string Identifier = "PexWizard";

        public IWindow LoadView(IWindowManager windowManager, IWizardView view)
        {
            WizardViewControl control = (WizardViewControl)view;
            return windowManager.Windows.Add(
                Identifier,
                control,
                "Pex Wizard");
        }

        public void UnloadView(IWindowManager windowManager)
        {
            windowManager.Windows.Remove(Identifier);
        }
    }
}
