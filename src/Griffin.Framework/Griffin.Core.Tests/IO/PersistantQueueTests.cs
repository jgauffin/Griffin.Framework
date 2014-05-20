using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.IO;
using Xunit;

namespace Griffin.Core.Tests.IO
{
    public class PersistantQueueTests : IDisposable
    {
        private string _queueName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());

        [Fact]
        public async Task SimpleTest()
        {
            var item = new TestDTO();
            var config = new PersistentQueueConfiguration(_queueName);
            config.DataDirectory = Path.GetTempPath();

            var sut = new PersistentQueue<TestDTO>(config);
            await sut.EnqueueAsync(item);
            var actual = await sut.DequeueAsync();

            actual.Id.Should().Be(item.Id);
            actual.UserName.Should().Be(item.UserName);
        }

        [Serializable]
        public class TestDTO
        {
            private static readonly Random _random = new Random();

            public TestDTO()
            {
                Id = Guid.NewGuid();
                UserName = _random.Next(0, 100000).ToString();
            }

            public Guid Id { get; set; }
            public string UserName { get; set; }
        }

        public void Dispose()
        {
            
        }
    }
}