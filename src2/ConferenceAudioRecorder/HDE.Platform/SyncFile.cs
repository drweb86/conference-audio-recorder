using System;
using System.IO;
using System.Threading;

namespace HDE.Platform.FileIO
{
    public sealed class SyncFile : IDisposable
    {
        private const string AlreadySynced = "Already synced";
        private const string NotSetOrNull = "Not set or null";

        private Mutex _mutex;
        private string _fileName = string.Empty;
        private bool _disposed;

        public string FileName => _fileName;

        public bool TrySyncFile(string fileName)
        {
            if (_mutex != null)
                throw new InvalidOperationException(AlreadySynced);

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException(NotSetOrNull, nameof(fileName));

            _fileName = fileName;

            try
            {
                _mutex = new Mutex(true, Path.GetFileName(fileName));
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            CleanResources();
            GC.SuppressFinalize(this);
        }

        ~SyncFile()
        {
            CleanResources();
        }

        private void CleanResources()
        {
            if (_mutex == null || _disposed)
                return;

            _mutex.Dispose();
            _mutex = null;
            _disposed = true;
        }
    }
}
