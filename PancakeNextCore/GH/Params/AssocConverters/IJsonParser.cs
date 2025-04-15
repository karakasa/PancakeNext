using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataType.AssocConverters;
public interface IJsonParser
{
    bool TryParseJson(string json, out GhAssocBase? assoc);
}
