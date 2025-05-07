using Hitorus.Data;
using System.Threading.Channels;

namespace Hitorus.Api.Download {
    public class DownloadEventBus : IEventBus<DownloadEventArgs> {
        private readonly Channel<DownloadEventArgs> _channel = Channel.CreateUnbounded<DownloadEventArgs>();
        public void Publish(DownloadEventArgs args) {
            _channel.Writer.TryWrite(args);
        }

        public ChannelReader<DownloadEventArgs> Subscribe() {
            return _channel.Reader;
        }
    }
}
