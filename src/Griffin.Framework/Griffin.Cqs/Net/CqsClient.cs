using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Net;
using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.MicroMsg.Serializers;

namespace Griffin.Cqs.Net
{
    public class CqsClient : ICommandBus, IEventBus, IQueryBus, IRequestReplyBus
    {
        private ChannelTcpClient<ClientResponse> _client;
        Dictionary<Guid, object> _response = new Dictionary<Guid, object>();

        public CqsClient()
        {
             _client= new ChannelTcpClient<ClientResponse>(new MicroMessageEncoder(new DataContractMessageSerializer()), new MicroMessageDecoder(new DataContractMessageSerializer()));
        }

        public CqsClient(Func<IMessageSerializer> serializer)
        {
            _client = new ChannelTcpClient<ClientResponse>(new MicroMessageEncoder(serializer()), new MicroMessageDecoder(serializer()));
        }


        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            //_client.SendAsync()
        }

        public Task PublishAsync<TApplicationEvent>(TApplicationEvent e) where TApplicationEvent : ApplicationEvent
        {
            throw new NotImplementedException();
        }

        public Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            throw new NotImplementedException();
        }

        public Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            throw new NotImplementedException();
        }
    }
}
