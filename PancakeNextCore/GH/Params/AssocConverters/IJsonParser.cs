using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params.AssocConverters;
public interface IJsonParser
{
    bool TryParseJson(string json, out GhAssocBase? assoc);
}
