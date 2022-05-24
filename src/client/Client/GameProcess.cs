using Vezel.Novadrop.Diagnostics;
using Windows.Win32.Foundation;
using Windows.Win32.System.DataExchange;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Novadrop.Client;

public abstract class GameProcess
{
    public event ReadOnlySpanAction<byte, nuint>? MessageReceived;

    public event ReadOnlySpanAction<byte, nuint>? MessageSent;

    public int Id => _process?.Id ?? throw new InvalidOperationException();

    int _started;

    ChildProcess? _process;

    private protected GameProcess()
    {
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    static unsafe LRESULT WindowProcedure(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        if (msg != WM_COPYDATA)
            return DefWindowProc(hWnd, msg, wParam, lParam);

        var cds = *(COPYDATASTRUCT*)(nint)lParam;
        var id = cds.dwData;
        var payload = new ReadOnlySpan<byte>(cds.lpData, (int)cds.cbData);

        var process = (GameProcess)GCHandle.FromIntPtr(GetWindowLongPtr(hWnd, 0)).Target!;

        process.MessageReceived?.Invoke(payload, id);

        var result = process.HandleWindowMessage(id, payload);

        if (result is not var (replyId, replyPayload))
            return (LRESULT)1;

        // We have to fire off the reply in a separate thread or we will cause a deadlock.
        _ = Task.Run(() =>
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

                _ = SendMessage((HWND)(nint)(nuint)wParam, msg, (nuint)(nint)hWnd, (nint)(&response));

                process.MessageSent?.Invoke(replySpan, replyId);
            }
        });

        return (LRESULT)1;
    }

    protected abstract void GetWindowConfiguration(out string className, out string windowName);

    protected abstract void GetProcessConfiguration(out string fileName, out string[] arguments);

    protected abstract (nuint Id, ReadOnlyMemory<byte> Payload)? HandleWindowMessage(
        nuint id, ReadOnlySpan<byte> payload);

    [SuppressMessage("", "CA1031")]
    public unsafe Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        return Interlocked.Exchange(ref _started, 1) != 1
            ? Task.Run(() =>
            {
                var code = 1;
                var exception = default(Exception);

                var thread = new Thread(() =>
                {
                    GetWindowConfiguration(out var className, out var windowName);
                    GetProcessConfiguration(out var fileName, out var arguments);

                    var handle = default(GCHandle);
                    var atom = 0;
                    var hwnd = default(HWND);

                    try
                    {
                        handle = GCHandle.Alloc(this);

                        fixed (char* ptr = className)
                            atom = RegisterClassEx(new WNDCLASSEXW
                            {
                                cbSize = (uint)sizeof(WNDCLASSEXW),
                                cbWndExtra = sizeof(nint),
                                lpszClassName = ptr,
                                lpfnWndProc = &WindowProcedure,
                            });

                        if (atom == 0)
                            throw new Win32Exception();

                        hwnd = CreateWindowEx(0, className, windowName, 0, 0, 0, 0, 0, default, null, null, null);

                        if ((nint)hwnd == 0)
                            throw new Win32Exception();

                        _ = SetWindowLongPtr(hwnd, 0, (IntPtr)handle);

                        _process = new(fileName, arguments, cancellationToken);

                        code = _process.Completion.GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    finally
                    {
                        if ((nint)hwnd != 0)
                            _ = DefWindowProc(hwnd, WM_CLOSE, 0, 0);

                        if (atom != 0)
                            _ = UnregisterClass(className, null);

                        if (handle.IsAllocated)
                            handle.Free();
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                return exception == null ? code : throw exception;
            })
            : throw new InvalidOperationException();
    }
}
