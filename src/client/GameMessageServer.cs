// SPDX-License-Identifier: 0BSD

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
            var hwnd = HWND.Null;

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

                hwnd = CreateWindowExW(
                    dwExStyle: 0,
                    className,
                    windowName,
                    dwStyle: 0,
                    X: 0,
                    Y: 0,
                    nWidth: 0,
                    nHeight: 0,
                    hWndParent: HWND.Null,
                    hMenu: null,
                    hInstance: null,
                    lpParam: null);

                if ((nint)hwnd == 0)
                    throw new Win32Exception();

                _ = SetWindowLongPtrW(hwnd, nIndex: 0, (nint)handle);

                _done.Task.GetAwaiter().GetResult();
            }
            finally
            {
                if ((nint)hwnd != 0)
                    _ = DefWindowProcW(hwnd, WM_CLOSE, wParam: 0, lParam: 0);

                if (atom != 0)
                    _ = UnregisterClassW(className, hInstance: null);

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

        var @this = Unsafe.As<GameMessageServer>(((GCHandle)GetWindowLongPtrW(hWnd, nIndex: 0)).Target!);

        @this.MessageReceived?.Invoke(payload, id);

        var result = @this.HandleWindowMessage(id, payload);

        if (result is not var (replyId, replyPayload))
            return (LRESULT)1;

        // We have to fire off the reply in a separate thread or we will cause a deadlock.
        _ = ThreadPool.UnsafeQueueUserWorkItem(
            static tup =>
            {
                var replySpan = tup.ReplyPayload.Span;

                fixed (byte* ptr = replySpan)
                {
                    var response = new COPYDATASTRUCT
                    {
                        dwData = tup.ReplyId,
                        lpData = ptr,
                        cbData = (uint)replySpan.Length,
                    };

                    _ = SendMessageW(
                        (HWND)(nint)(nuint)tup.Sender, tup.Message, (nuint)(nint)tup.Receiver, (nint)(&response));

                    tup.This.MessageSent?.Invoke(replySpan, tup.ReplyId);
                }
            },
            (This: @this, Sender: wParam, Receiver: hWnd, Message: msg, ReplyId: replyId, ReplyPayload: replyPayload),
            preferLocal: true);

        return (LRESULT)1;
    }
}
