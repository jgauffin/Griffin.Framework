using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.IO;
using Griffin.Net.Protocols.Serializers.Mono;
using Xunit;

namespace Griffin.Core.Tests.IO
{
    public class OneFilePerItemQueueTests
    {
        
        public class User
        {
            public string Id { get; set; }
            public object Data { get; set; }
        }

        [Fact]
        public void create_directory()
        {

            string id = Guid.NewGuid().ToString();
            var sut = new OneFilePerItemQueue<User>(id);
            try
            {
                sut.Start();
                sut.EnqueueAsync(new User() {Data = "mamma"}).Wait();
            }
            finally
            {
                Directory.Delete(Path.Combine(sut.DataDirectory, id), true);
            }
           

        }

        [Fact]
        public void integration_test()
        {
            

            var sut = new OneFilePerItemQueue<User>("TestQueue");
            sut.Start();
            sut.EnqueueAsync(new User() {Data = "mamma"}).Wait();
            sut.Start();
            var t = sut.DequeueAsync();
            t.Wait();
            var actual = t.Result;

        }
    }
}
