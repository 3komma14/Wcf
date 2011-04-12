using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Seterlund.Wcf
{
    public interface IChannelFactoryManager
    {
        ChannelFactory<T> GetFactory<T>();
        ChannelFactory<T> GetFactory<T>(ClientCredentials clientCredentials);
        ChannelFactory<T> GetFactory<T, TBinding>() where TBinding : Binding;
    }
}