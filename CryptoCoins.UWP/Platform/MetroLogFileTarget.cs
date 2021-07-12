using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using MetroLog;
using MetroLog.Layouts;
using MetroLog.Targets;

namespace CryptoCoins.UWP.Platform
{
    public class MetroLogFileTarget : FileTargetBase
    {
        private static StorageFolder logFolder;

        public MetroLogFileTarget()
            : this(new SingleLineLayout())
        {
            RetainDays = 1;
        }

        protected MetroLogFileTarget(Layout layout) : base(layout)
        {
            FileNamingParameters.IncludeLevel = false;
            FileNamingParameters.IncludeLogger = false;
            FileNamingParameters.IncludeSequence = false;
            FileNamingParameters.IncludeSession = false;
            FileNamingParameters.IncludeTimestamp = FileTimestampMode.Date;
            FileNamingParameters.CreationMode = FileCreationMode.AppendIfExisting;
        }

        public static async Task<StorageFolder> EnsureInitializedAsync()
        {
            if (logFolder == null)
            {
                var root = ApplicationData.Current.LocalFolder;

                logFolder = await root.CreateFolderAsync(LogFolderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
            }
            return logFolder;
        }

        protected override async Task<Stream> GetCompressedLogsInternal()
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            var ms = new MemoryStream();

            using (var a = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var file in await logFolder.GetFilesAsync().AsTask().ConfigureAwait(false))
                {
                    var zipFile = a.CreateEntry(file.Name);
                    using (var writer = new StreamWriter(zipFile.Open()))
                    {
                        await writer.WriteAsync(await FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false)).ConfigureAwait(false);
                    }
                }
            }

            ms.Position = 0;
            return ms;
        }

        protected override Task EnsureInitialized()
        {
            return EnsureInitializedAsync();
        }

        protected sealed override async Task DoCleanup(Regex pattern, DateTime threshold)
        {
            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, null);

            queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.None,
                new[] {"System.DateCreated"});

            var queryResults = ((StorageFolder) logFolder).CreateFileQueryWithOptions(queryOptions);
            var count = await queryResults.GetItemCountAsync().AsTask().ConfigureAwait(false);
            if (count > 150)
            {
                InternalLogger.Current.Warn($"Too much files to iterate {count}. Deleting folder");
                //Recreate folder. Trying to itarate a lot of files can lead to application beeing closed at splash screen
                await logFolder.DeleteAsync().AsTask().ConfigureAwait(false);
                await EnsureInitializedAsync().ConfigureAwait(false);
            }
            else
            {
                var files = await queryResults.GetFilesAsync().AsTask().ConfigureAwait(false);
                var toDelete = files.Where(file => pattern.Match(file.Name).Success && file.DateCreated <= threshold);

                foreach (var file in toDelete)
                {
                    try
                    {
                        await file.DeleteAsync().AsTask().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Current.Warn($"Failed to delete '{file.Path}'.", ex);
                    }
                }
            }
        }

        protected sealed override async Task<LogWriteOperation> DoWriteAsync(StreamWriter streamWriter, string contents, LogEventInfo entry)
        {
            // Write contents
            await WriteTextToFileCore(streamWriter, contents).ConfigureAwait(false);

            // return...
            return new LogWriteOperation(this, entry, true);
        }

        protected Task WriteTextToFileCore(StreamWriter stream, string contents)
        {
            return stream.WriteLineAsync(contents);
        }

        protected override async Task<Stream> GetWritableStreamForFile(string fileName)
        {
            var file = await logFolder.CreateFileAsync(fileName,
                FileNamingParameters.CreationMode == FileCreationMode.AppendIfExisting ? CreationCollisionOption.OpenIfExists : CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);

            var stream = await file.OpenStreamForWriteAsync().ConfigureAwait(false);

            if (FileNamingParameters.CreationMode == FileCreationMode.AppendIfExisting)
            {
                // Make sure we're at the end of the stream for appending
                stream.Seek(0, SeekOrigin.End);
            }

            return stream;
        }
    }
}
