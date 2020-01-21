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
    public class WizardSettings
    {
        public WizardSettings(string wizardFileName)
        {
            if (String.IsNullOrEmpty(wizardFileName))
                throw new ArgumentNullException("wizardFileName");

            this.WizardFileName = wizardFileName;
        }

        public string WizardFileName { get; private set; }
        public bool AssertInconclusive { get; set; }
    }
}
