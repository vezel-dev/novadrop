namespace Vezel.Novadrop.Helpers;

sealed class DataSheetWatcher : IDisposable
{
    const string SheetFilter = "?*-?*.xml";

    readonly ConcurrentQueue<(string, DataSheetState)> _queue = new();

    readonly DirectoryInfo _directory;

    FileSystemWatcher _fsw;

    public DataSheetWatcher(DirectoryInfo directory)
    {
        _directory = directory;
        _fsw = CreateWatcher(true);
    }

    ~DataSheetWatcher()
    {
        Dispose();
    }

    public void Dispose()
    {
        _fsw?.Dispose();
        GC.SuppressFinalize(this);
    }

    FileSystemWatcher CreateWatcher(bool initial)
    {
        var fsw = new FileSystemWatcher(_directory.FullName)
        {
            Filter = SheetFilter,
            IncludeSubdirectories = true,
            NotifyFilter =
                NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            InternalBufferSize = ushort.MaxValue,
        };

        fsw.Error += HandleError;
        fsw.Created += HandleCreated;
        fsw.Changed += HandleChanged;
        fsw.Deleted += HandleDeleted;
        fsw.Renamed += HandleRenamed;

        fsw.EnableRaisingEvents = true;

        if (initial)
            foreach (var file in _directory.EnumerateFiles(SheetFilter, SearchOption.AllDirectories))
                Enqueue(file.FullName, DataSheetState.Created);

        return fsw;
    }

    void Enqueue(string path, DataSheetState state)
    {
        _queue.Enqueue((path, state));
    }

    void HandleCreated(object sender, FileSystemEventArgs e)
    {
        Enqueue(e.FullPath, DataSheetState.Created);
    }

    void HandleChanged(object sender, FileSystemEventArgs e)
    {
        Enqueue(e.FullPath, DataSheetState.Modified);
    }

    void HandleDeleted(object sender, FileSystemEventArgs e)
    {
        Enqueue(e.FullPath, DataSheetState.Deleted);
    }

    void HandleRenamed(object sender, RenamedEventArgs e)
    {
        Enqueue(e.OldFullPath, DataSheetState.Deleted);
        Enqueue(e.FullPath, DataSheetState.Created);
    }

    void HandleError(object sender, ErrorEventArgs e)
    {
        if (e.GetException() is not Win32Exception)
        {
            // This should be a recoverable error.

            _fsw.EnableRaisingEvents = false;

            _fsw.Renamed -= HandleRenamed;
            _fsw.Deleted -= HandleDeleted;
            _fsw.Changed -= HandleChanged;
            _fsw.Created -= HandleCreated;
            _fsw.Error -= HandleError;

            _fsw.Dispose();

            _fsw = CreateWatcher(false);
        }
    }

    public bool TryDequeue(out (string Path, DataSheetState State) change)
    {
        return _queue.TryDequeue(out change);
    }
}
