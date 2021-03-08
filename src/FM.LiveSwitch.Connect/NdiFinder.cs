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

        protected NDI.Finder finder;
        protected const int findDuration = 5;

        public NdiFinder(NdiFindOptions options)
        {
        }
        
        protected void Start()
        {
            Console.WriteLine($"Looking for NDI device sources for {findDuration} seconds.");
            finder = new NDI.Finder(true);
            finder.Sources.CollectionChanged += HandleNdiSourcesChanged;
        }

        public async Task<int> Run()
        {
            Start();
            await Task.Delay(findDuration*1000);
            Stop();
            return 0;
        }

        protected void Stop()
        {
            if (finder != null)
            {
                finder.Sources.CollectionChanged -= HandleNdiSourcesChanged;
                finder.Dispose();
                finder = null;
            }
        }

        protected void HandleNdiSourcesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                Console.WriteLine($"Found {e.NewItems.Count} new NDI device sources:");
                foreach(NDI.Source source in e.NewItems)
                {
                    Console.WriteLine("  " + source.Name);
                }
            }
        }
    }
}
