using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Signals;
using NCrunch.Framework;
using Xunit;

namespace Griffin.Core.Tests.Signals
{
    [ExclusivelyUses("SignalSubmitter")]
    public class SignalSubmitterTests
    {
        [Fact]
        public void TestUpload()
        {
            SignalSubmitter.Configure("AppName", new Uri("http://localhost:8083"));
            SignalSubmitter.UploadAllSignals();

            var signal = Signal.Create("MySignal");
            signal.Raise("Somethign is failing");
        }
    }
}
