using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.sync.WatcherHelpers;
using starsky.foundation.sync.WatcherInterfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherServices
{
	/// <summary>
	/// @see: http://web.archive.org/web/20210415105229/https://petermeinl.wordpress.com/2015/05/18/tamed-filesystemwatcher/
	/// </summary>
	[Service(typeof(IFileSystemWatcherWrapper), InjectionLifetime = InjectionLifetime.Singleton)]
	public class BufferingFileSystemWatcher : Component, IFileSystemWatcherWrapper
    {
        private readonly FileSystemWatcher _containedFsw = null;

        private FileSystemEventHandler _onExistedHandler = null;
        private FileSystemEventHandler _onAllChangesHandler = null;

        private FileSystemEventHandler _onCreatedHandler = null;
        private FileSystemEventHandler _onChangedHandler = null;
        private FileSystemEventHandler _onDeletedHandler = null;
        private RenamedEventHandler _onRenamedHandler = null;

        private ErrorEventHandler _onErrorHandler = null;

        //We use a single buffer for all change types. Alternatively we could use one buffer per event type, costing additional enumerate tasks.
        private BlockingCollection<FileSystemEventArgs> _fileSystemEventBuffer = null;
        private CancellationTokenSource _cancellationTokenSource = null;

        #region Contained FileSystemWatcher
        
        /// <devdoc>
        /// Features:
        /// - Buffers FileSystemWatcher events in a BlockinCollection to prevent InternalBufferOverflowExceptions.
        /// - Does not break the original FileSystemWatcher API.
        /// - Supports reporting existing files via a new Existed event.
        /// - Supports sorting events by oldest (existing) file first.
        /// - Supports an new event Any reporting any FSW change.
        /// - Offers the Error event in Win Forms designer (via [Browsable[true)]
        /// - Does not prevent duplicate files occuring.
        /// Notes:
        ///   We contain FilSystemWatcher to follow the prinicple composition over inheritance
        ///   and because System.IO.FileSystemWatcher is not designed to be inherited from:
        ///   Event handlers and Dispose(disposing) are not virtual.
        /// </devdoc>
        public BufferingFileSystemWatcher()
        {
	        _containedFsw = new FileSystemWatcher();
        }

        public BufferingFileSystemWatcher(string path)
        {
	        _containedFsw = new FileSystemWatcher(path, "*.*");
        }
        
        public BufferingFileSystemWatcher(string path, string filter)
        {
	        _containedFsw = new FileSystemWatcher(path, filter);
        }
        
        public BufferingFileSystemWatcher(FileSystemWatcher fileSystemWatcher)
        {
	        _containedFsw = fileSystemWatcher;
        }

        public bool EnableRaisingEvents
        {
            get
            {
                return _containedFsw.EnableRaisingEvents;
            }
            set
            {
                if (_containedFsw.EnableRaisingEvents == value) return;

                StopRaisingBufferedEvents();
                _cancellationTokenSource = new CancellationTokenSource();

                //We EnableRaisingEvents, before NotifyExistingFiles
                //  to prevent missing any events
                //  accepting more duplicates (which may occure anyway).
                _containedFsw.EnableRaisingEvents = value;
                if (value)
                    RaiseBufferedEventsUntilCancelled();
            }
        }

        public string Filter
        {
            get { return _containedFsw.Filter; }
            set { _containedFsw.Filter = value; }
        }

        public bool IncludeSubdirectories
        {
            get { return _containedFsw.IncludeSubdirectories; }
            set { _containedFsw.IncludeSubdirectories = value; }
        }

        public int InternalBufferSize
        {
            get { return _containedFsw.InternalBufferSize; }
            set { _containedFsw.InternalBufferSize = value; }
        }

        public NotifyFilters NotifyFilter
        {
            get { return _containedFsw.NotifyFilter; }
            set => _containedFsw.NotifyFilter = value;
        }

        public string Path
        {
            get { return _containedFsw.Path; }
            set { _containedFsw.Path = value; }
        }

        public ISynchronizeInvoke SynchronizingObject
        {
            get { return _containedFsw.SynchronizingObject; }
            set { _containedFsw.SynchronizingObject = value; }
        }

        public override ISite Site
        {
            get { return _containedFsw.Site; }
            set { _containedFsw.Site = value; }
        }

        #endregion

        [DefaultValue(false)]
        public bool OrderByOldestFirst { get; set; } = false;

        public int EventQueueCapacity { get; set;  } = int.MaxValue;

        // New BufferingFileSystemWatcher specific events
        public event FileSystemEventHandler Existed
        {
            add
            {
                _onExistedHandler += value;
            }
            remove
            {
                _onExistedHandler -= value;
            }
        }

        public event FileSystemEventHandler All
        {
            add
            {
                if (_onAllChangesHandler == null)
                {
                    _containedFsw.Created += BufferEvent;
                    _containedFsw.Changed += BufferEvent;
                    _containedFsw.Renamed += BufferEvent;
                    _containedFsw.Deleted += BufferEvent;
                }
                _onAllChangesHandler += value;
            }
            remove
            {
                _containedFsw.Created -= BufferEvent;
                _containedFsw.Changed -= BufferEvent;
                _containedFsw.Renamed -= BufferEvent;
                _containedFsw.Deleted -= BufferEvent;
                _onAllChangesHandler -= value;
            }
        }


        // region Standard FSW events
        
        //- The _fsw events add to the buffer.
        //- The public events raise from the buffer to the consumer.
        public event FileSystemEventHandler Created
        {
            add
            {
                if (_onCreatedHandler == null)
                    _containedFsw.Created += BufferEvent;
                _onCreatedHandler += value;
            }
            remove
            {
                _containedFsw.Created -= BufferEvent;
                _onCreatedHandler -= value;
            }
        }

        public event FileSystemEventHandler Changed
        {
            add
            {
                if (_onChangedHandler == null)
                    _containedFsw.Changed += BufferEvent;
                _onChangedHandler += value;
            }
            remove
            {
                _containedFsw.Changed -= BufferEvent;
                _onChangedHandler -= value;
            }
        }

        public event FileSystemEventHandler Deleted
        {
            add
            {
                if (_onDeletedHandler == null)
                    _containedFsw.Deleted += BufferEvent;
                _onDeletedHandler += value;
            }
            remove
            {
                _containedFsw.Deleted -= BufferEvent;
                _onDeletedHandler -= value;
            }
        }

        public event RenamedEventHandler Renamed
        {
            add
            {
                if (_onRenamedHandler == null)
                    _containedFsw.Renamed += BufferEvent;
                _onRenamedHandler += value;
            }
            remove
            {
                _containedFsw.Renamed -= BufferEvent;
                _onRenamedHandler -= value;
            }
        }

        internal void BufferEvent(object _, FileSystemEventArgs e)
        {
	        if ( _fileSystemEventBuffer.TryAdd(e) ) return;
	        var ex = new EventQueueOverflowException($"Event queue size {_fileSystemEventBuffer.BoundedCapacity} events exceeded.");
	        InvokeHandler(_onErrorHandler, new ErrorEventArgs(ex));
        }

        internal void StopRaisingBufferedEvents(object _ = null, EventArgs __ = null)
        {
            _cancellationTokenSource?.Cancel();
            _fileSystemEventBuffer = new BlockingCollection<FileSystemEventArgs>(EventQueueCapacity);
        }

        public event ErrorEventHandler Error
        {
            add
            {
                if (_onErrorHandler == null)
                    _containedFsw.Error += BufferingFileSystemWatcher_Error;
                _onErrorHandler += value;
            }
            remove
            {
                if (_onErrorHandler == null)
                    _containedFsw.Error -= BufferingFileSystemWatcher_Error;
                _onErrorHandler -= value;
            }
        }

        internal void BufferingFileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
            InvokeHandler(_onErrorHandler, e);
        }
        
        // end standard events

        internal WatcherChangeTypes? RaiseBufferedEventsUntilCancelledInLoop(
	        FileSystemEventArgs fileSystemEventArgs)
        {
	        if (_onAllChangesHandler != null)
		        InvokeHandler(_onAllChangesHandler, fileSystemEventArgs);
	        else
	        {
		        switch (fileSystemEventArgs.ChangeType)
		        {
			        case WatcherChangeTypes.Created:
				        InvokeHandler(_onCreatedHandler, fileSystemEventArgs);
				        break;
			        case WatcherChangeTypes.Changed:
				        InvokeHandler(_onChangedHandler, fileSystemEventArgs);
				        break;
			        case WatcherChangeTypes.Deleted:
				        InvokeHandler(_onDeletedHandler, fileSystemEventArgs);
				        break;
			        case WatcherChangeTypes.Renamed:
				        InvokeHandler(_onRenamedHandler, fileSystemEventArgs as RenamedEventArgs);
				        break;
		        }
		        return fileSystemEventArgs.ChangeType;
	        }

	        return null;
        }

        private void RaiseBufferedEventsUntilCancelled()
        {
            Task.Run(() =>
            {
	            try
	            {
		            if ( _onExistedHandler != null ||
		                 _onAllChangesHandler != null )
			            NotifyExistingFiles();

		            foreach ( FileSystemEventArgs fileSystemEventArgs in
		                     _fileSystemEventBuffer.GetConsumingEnumerable(
			                     _cancellationTokenSource.Token) )
		            {
			            RaiseBufferedEventsUntilCancelledInLoop(
				            fileSystemEventArgs);
		            }
	            }
	            catch ( OperationCanceledException )
	            {
		            // ignore
	            } 
                catch (Exception ex)
                {
                    BufferingFileSystemWatcher_Error(this, new ErrorEventArgs(ex));
                }
            });
        }

        internal void NotifyExistingFiles()
        {
            var searchSubDirectoriesOption = (IncludeSubdirectories) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (OrderByOldestFirst)
            {
                var sortedFileInfos = from fi in new DirectoryInfo(Path).GetFiles(Filter, searchSubDirectoriesOption)
                                      orderby fi.LastWriteTime ascending
                                      select fi;
                foreach (var fi in sortedFileInfos)
                {
                    InvokeHandler(_onExistedHandler, new FileSystemEventArgs(WatcherChangeTypes.All, fi.DirectoryName, fi.Name));
                    InvokeHandler(_onAllChangesHandler, new FileSystemEventArgs(WatcherChangeTypes.All,fi.DirectoryName, fi.Name));
                }
            }
            else
            {
                foreach (var fsi in new DirectoryInfo(Path).EnumerateFileSystemInfos(Filter, searchSubDirectoriesOption))
                {
                    InvokeHandler(_onExistedHandler, new FileSystemEventArgs(WatcherChangeTypes.All, System.IO.Path.GetDirectoryName(fsi.FullName), fsi.Name ));
                    InvokeHandler(_onAllChangesHandler, new FileSystemEventArgs(WatcherChangeTypes.All, System.IO.Path.GetDirectoryName(fsi.FullName), fsi.Name));
                }
            }
        }

        // InvokeHandlers
        // Automatically raise event in calling thread when _fsw.SynchronizingObject is set. Ex: When used as a component in Win Forms.
        //  remove redundancy. I don't understand how to cast the specific *EventHandler to a generic Delegate, EventHandler, Action or whatever.
        internal bool? InvokeHandler(FileSystemEventHandler eventHandler, FileSystemEventArgs e)
        {
	        if ( eventHandler == null ) return null;
	        if ( _containedFsw.SynchronizingObject != null && _containedFsw
		            .SynchronizingObject.InvokeRequired )
	        {
		        _containedFsw.SynchronizingObject.BeginInvoke(eventHandler, new object[] { this, e });
		        return true;
	        }

	        eventHandler(this, e);
	        return false;
        }
        internal bool? InvokeHandler(RenamedEventHandler eventHandler, RenamedEventArgs e)
        {
	        if ( eventHandler == null ) return null;
	        if ( _containedFsw.SynchronizingObject != null && this._containedFsw
		            .SynchronizingObject.InvokeRequired )
	        {
		        _containedFsw.SynchronizingObject.BeginInvoke(eventHandler, new object[] { this, e });
		        return true;
	        }

	        eventHandler(this, e);
	        return false;
        }
        internal bool? InvokeHandler(ErrorEventHandler eventHandler, ErrorEventArgs e)
        {
	        if ( eventHandler == null ) return null;
	        if ( _containedFsw.SynchronizingObject != null && this._containedFsw
		            .SynchronizingObject.InvokeRequired )
	        {
		        _containedFsw.SynchronizingObject.BeginInvoke(eventHandler, new object[] { this, e });
		        return true;
	        }

            eventHandler(this, e);
            return false;
        }
        // end InvokeHandlers

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Cancel();
                _containedFsw?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
