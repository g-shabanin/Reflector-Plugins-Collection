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
    interface IShell
    {
        void Execute(string fileName, string parameters, Action<string> consoleSink);
    }
}
