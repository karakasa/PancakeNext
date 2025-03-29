using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.DataTypes.Converters;
public interface IJsonParser
{
    bool TryParseJson(string json, out Association? assoc);
}
