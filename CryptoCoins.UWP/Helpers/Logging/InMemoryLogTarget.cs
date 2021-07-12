using System.Collections.Generic;
using System.Linq;
using MetroLog;
using MetroLog.Layouts;
using MetroLog.Targets;

namespace CryptoCoins.UWP.Helpers.Logging
{
    public class InMemoryLogTarget : SyncTarget
    {
        private const int DefaultMaxLines = 300;
        private readonly LinkedList<string> _logEntries;
        private readonly int _maxLines;

        public InMemoryLogTarget(IEnumerable<string> source, int maxLines = DefaultMaxLines) : base(new SingleLineLayout())
        {
            _maxLines = maxLines;
            _logEntries = new LinkedList<string>(source.Take(maxLines));
        }

        public InMemoryLogTarget(int maxLines = DefaultMaxLines)
            : base(new SingleLineLayout())
        {
            _maxLines = maxLines;
            _logEntries = new LinkedList<string>();
        }

        public InMemoryLogTarget(Layout layout, int maxLines = DefaultMaxLines)
            : base(layout)
        {
            _maxLines = maxLines;
            _logEntries = new LinkedList<string>();
        }

        public IEnumerable<string> LogLines => _logEntries;

        protected override void Write(LogWriteContext context, LogEventInfo entry)
        {
            lock (this)
            {
                Write(Layout.GetFormattedString(context, entry));
            }
        }

        public void Write(string line)
        {
            _logEntries.AddLast(line);
            if (_logEntries.Count > _maxLines)
            {
                _logEntries.RemoveFirst();
            }
        }
    }
}
