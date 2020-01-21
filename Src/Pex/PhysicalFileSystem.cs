// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Reflector.Pex
{
    class PhysicalFileSystem :
        IFileSystem
    {
        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}
