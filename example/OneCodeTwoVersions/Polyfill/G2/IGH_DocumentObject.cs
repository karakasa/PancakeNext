﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public interface IGH_DocumentObject : IGH_InstanceDescription
{
    Guid ComponentGuid { get; }
}
