﻿using Grasshopper2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCodeTwoVersions.Polyfill;
public interface IGH_DataTree
{
    bool MergeWithParameter(IGH_Param param);
    ITree To2();
}
