using System.Threading.Channels;

namespace Hitorus.Data {
    public interface IEventBus<T> {
        void Publish(T eventData);
        ChannelReader<T> Subscribe();
    }
}
