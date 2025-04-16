using System.Threading;

namespace PancakeNextCore.Utility;

internal sealed class AtomicBool(bool initValue = false)
{
    private int _threadSafeBoolBackValue = initValue ? 1 : 0;

    public bool Value
    {
        get => Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 1) == 1;
        set
        {
            if (value) Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 0);
            else Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 0, 1);
        }
    }

    public static implicit operator AtomicBool(bool v) => new(v);
    public static implicit operator bool(AtomicBool v) => v.Value;
}
