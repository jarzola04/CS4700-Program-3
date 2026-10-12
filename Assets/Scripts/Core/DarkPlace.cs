using System;

public static class DarkPlace
{
    public static bool IsDark { get; private set; }
    public static event Action<bool> Changed;

    public static void Set(bool dark)
    {
        if (dark == IsDark) return;
        IsDark = dark;
        Changed?.Invoke(dark);
    }
}
