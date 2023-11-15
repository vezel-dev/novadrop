using Windows.Win32.Foundation;
using Windows.Win32.System.DataExchange;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Novadrop.Client;

[SuppressMessage("", "CA1063")]
public abstract class GameMessageServer : IDisposable
{
    public event ReadOnlySpanAction<byte, nuint>? MessageReceived;

    public event ReadOnlySpanAction<byte, nuint>? MessageSent;

    private readonly TaskCompletionSource _done = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly Thread _thread;

    private int _disposed;

    private protected unsafe GameMessageServer()
    {
        _thread = new Thread(() =>
        {
            GetWindowConfiguration(out var className, out var windowName);

            var handle = default(GCHandle);
            var atom = 0;
            var hwnd = default(HWND);

            try
            {
                handle = GCHandle.Alloc(this);

                fixed (char* ptr = className)
                    atom = RegisterClassExW(new WNDCLASSEXW
                    {
                        cbSize = (uint)sizeof(WNDCLASSEXW),
                        cbWndExtra = sizeof(nint),
                        lpszClassName = ptr,
                        lpfnWndProc = &WindowProcedure,
                    });

                if (atom == 0)
                    throw new Win32Exception();

                hwnd = CreateWindowExW(0, className, windowName, 0, 0, 0, 0, 0, default, null, null, null);

                if ((nint)hwnd == 0)
                    throw new Win32Exception();

                _ = SetWindowLongPtrW(hwnd, 0, (nint)handle);

                _done.Task.GetAwaiter().GetResult();
            }
            finally
            {
                if ((nint)hwnd != 0)
                    _ = DefWindowProcW(hwnd, WM_CLOSE, 0, 0);

                if (atom != 0)
                    _ = UnregisterClassW(className, null);

                if (handle.IsAllocated)
                    handle.Free();
            }
        });

        _thread.SetApartmentState(ApartmentState.STA);
    }

    ~GameMessageServer()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        _done.SetResult();
        _thread.Join();

        GC.SuppressFinalize(this);
    }

    private protected void Start()
    {
        _thread.Start();
    }

    private protected abstract void GetWindowConfiguration(out string className, out string windowName);

    private protected abstract (nuint Id, ReadOnlyMemory<byte> Payload)? HandleWindowMessage(
        nuint id, scoped ReadOnlySpan<byte> payload);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe LRESULT WindowProcedure(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        if (msg != WM_COPYDATA)
            return DefWindowProcW(hWnd, msg, wParam, lParam);

        var cds = *(COPYDATASTRUCT*)(nint)lParam;
        var id = cds.dwData;
        var payload = new ReadOnlySpan<byte>(cds.lpData, (int)cds.cbData);

        var process = Unsafe.As<GameMessageServer>(((GCHandle)GetWindowLongPtrW(hWnd, 0)).Target!);

        process.MessageReceived?.Invoke(payload, id);

        var result = process.HandleWindowMessage(id, payload);

        if (result is not var (replyId, replyPayload))
            return (LRESULT)1;

        // We have to fire off the reply in a separate thread or we will cause a deadlock.
        _ = ThreadPool.UnsafeQueueUserWorkItem(
            _ =>
            {
                var replySpan = replyPayload.Span;

                fixed (byte* ptr = replySpan)
                {
                    var response = new COPYDATASTRUCT
                    {
                        dwData = replyId,
                        lpData = ptr,
                        cbData = (uint)replyPayload.Length,
                    };

                    _ = SendMessageW((HWND)(nint)(nuint)wParam, msg, (nuint)(nint)hWnd, (nint)(&response));

                    process.MessageSent?.Invoke(replySpan, replyId);
                }
            },
            null);

        return (LRESULT)1;
    }
}
