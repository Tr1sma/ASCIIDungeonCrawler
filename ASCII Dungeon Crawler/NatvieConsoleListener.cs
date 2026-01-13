using System.Runtime.InteropServices;

public class NativeConsoleListener
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool ReadConsoleInput(
        IntPtr hConsoleInput,
        [Out] INPUT_RECORD[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetNumberOfConsoleInputEvents(
        IntPtr hConsoleInput,
        out uint lpNumberOfEvents);

    const int STD_INPUT_HANDLE = -10;
    const ushort KEY_EVENT = 0x0001;
    const ushort WINDOW_BUFFER_SIZE_EVENT = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    public struct COORD { public short X; public short Y; }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_RECORD
    {
        [FieldOffset(0)] public ushort EventType;
        [FieldOffset(4)] public KEY_EVENT_RECORD KeyEvent;
        [FieldOffset(4)] public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEY_EVENT_RECORD
    {
        public bool bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public char uChar;
        public uint dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOW_BUFFER_SIZE_RECORD { public COORD dwSize; }

    private IntPtr _stdInHandle;

    public NativeConsoleListener()
    {
        _stdInHandle = GetStdHandle(STD_INPUT_HANDLE);
    }

    /// <summary>
    /// Pollt den Input Bufer. Callbacks für Key press oder Resize.
    /// </summary>
    public void ProcessInput(Action<ConsoleKey> onKeyDown, Action onResize)
    {
        uint numEvents;
        if (!GetNumberOfConsoleInputEvents(_stdInHandle, out numEvents) || numEvents == 0)
            return;

        INPUT_RECORD[] recordBuffer = new INPUT_RECORD[numEvents];
        uint numRead;

        if (ReadConsoleInput(_stdInHandle, recordBuffer, numEvents, out numRead))
        {
            for (int i = 0; i < numRead; i++)
            {
                switch (recordBuffer[i].EventType)
                {
                    case KEY_EVENT:
                        var keyEvent = recordBuffer[i].KeyEvent;
                        if (keyEvent.bKeyDown)
                        {
                            onKeyDown((ConsoleKey)keyEvent.wVirtualKeyCode);
                        }
                        break;

                    case WINDOW_BUFFER_SIZE_EVENT:
                        onResize?.Invoke();
                        break;
                }
            }
        }
    }
}