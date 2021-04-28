using System;
using System.Threading;
using System.Collections.Specialized;
using System.Threading.Tasks;
using NDI = NewTek.NDI;

namespace FM.LiveSwitch.Connect
{
    class NdiFinder
    {
        static ILog _Log = Log.GetLogger(typeof(NdiCapturer));

        protected NDI.Finder _NdiFinder;
        public int FindDuration = 5;

        public NdiFinder(NdiFindOptions options)
        {
        }
        
        protected void Start()
        {
            Console.Error.WriteLine($"Looking for NDI device sources for {FindDuration} seconds.");
            _NdiFinder = new NDI.Finder(true);
            _NdiFinder.Sources.CollectionChanged += HandleNdiSourcesChanged;
        }

        public async Task<int> Run()
        {
            Start();
            await Task.Delay(FindDuration*1000).ConfigureAwait(false);
            Stop();
            return 0;
        }

        protected void Stop()
        {
            if (_NdiFinder != null)
            {
                _NdiFinder.Sources.CollectionChanged -= HandleNdiSourcesChanged;
                _NdiFinder.Dispose();
                _NdiFinder = null;
            }
        }

        protected void HandleNdiSourcesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                Console.Error.WriteLine($"Found {e.NewItems.Count} new NDI device sources:");
                foreach(NDI.Source source in e.NewItems)
                {
                    Console.WriteLine("  " + source.Name);
                }
            }
        }
    }
}
