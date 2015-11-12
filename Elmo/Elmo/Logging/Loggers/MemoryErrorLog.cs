using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elmo.Logging.Loggers
{
    public class MemoryErrorLog : IErrorLog
    {
        public static int DefaultSize { get; } = 15;
        public static int MaximumSize { get; } = 500;

        private static readonly FixedSizeCollection Entries = new FixedSizeCollection();

        public MemoryErrorLog() : this(DefaultSize) { }

        public MemoryErrorLog(int size)
        {
            if (size < 0 || size > MaximumSize)
                throw new ArgumentOutOfRangeException(nameof(size), size, $"Size must be between 0 and {MaximumSize}.");

            Entries.Size = size;
        }

        public string Log(Error error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));
            
            var id = GenerateId();
            Entries.Add(new ErrorLogEntry(id, error));
            return id;
        }

        private static string GenerateId() => Guid.NewGuid().ToString();

        public Task<string> LogAsync(Error error)
        {
            return Task.FromResult(Log(error));
        }

        public ErrorLogEntry GetError(string id)
        {
            return Entries[id];
        }

        public Task<ErrorLogEntry> GetErrorAsync(string id)
        {
            return Task.FromResult(GetError(id));
        }

        public IList<ErrorLogEntry> GetErrors(int pageIndex, int pageSize)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "Page index below zero.");

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size below zero.");

            var totalCount = Entries.Count;

            var startIndex = pageIndex * pageSize;
            var endIndex = Math.Min(startIndex + pageSize, totalCount);
            var count = Math.Max(0, endIndex - startIndex);

            return Entries.Subset(startIndex, count);
        }

        public Task<IList<ErrorLogEntry>> GetErrorsAsync(int pageIndex, int pageSize)
        {
            return Task.FromResult(GetErrors(pageIndex, pageSize));
        }

        public int GetTotalErrorCount()
        {
            return Entries.Count;
        }

        public Task<int> GetTotalErrorCountAsync()
        {
            return Task.FromResult(GetTotalErrorCount());
        }

        public string Name => "In-Memory Error Log";

        public string ApplicationName => AppDomain.CurrentDomain.FriendlyName;

        private class FixedSizeCollection
        {
            private readonly object lockObject = new object();
            private readonly List<ErrorLogEntry> list = new List<ErrorLogEntry>();
            private int size;

            public IList<ErrorLogEntry> Subset(int startIndex, int count)
            {
                lock (lockObject)
                {
                    return list.GetRange(startIndex, count);
                }
            }

            public ErrorLogEntry this[string id]
            {
                get
                {
                    lock (lockObject)
                    {
                        return list.Find(entry => entry.Id == id);
                    }
                }
            }

            public void Add(ErrorLogEntry entry)
            {
                lock (lockObject)
                {
                    if (list.Count == size)
                        list.RemoveAt(0);

                    list.Add(entry);
                }
            }

            public int Count
            {
                get
                {
                    lock (lockObject)
                    {
                        return list.Count;
                    }
                }
            }

            public int Size
            {
                set
                {
                    lock (lockObject)
                    {
                        size = value;
                    }
                }
            }
        }
    }
}
