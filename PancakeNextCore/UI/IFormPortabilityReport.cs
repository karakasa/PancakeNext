using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.UI;

public interface IFormPortabilityReport
{
    public IEnumerable<Guid> SelectedObjects { get; }
    public void RefreshContent();
    public ResultEntry[] Results { get; }
}
