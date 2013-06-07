using System;
using System.Diagnostics;

namespace Rexster.Tests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Properties;

    [TestClass]
    public class TimingTests
    {
        private RexProClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = new RexProClient(Settings.Default.RexProHost, Settings.Default.RexProPort);
        }

        [TestMethod]
        public void QueryScalarValue()
        {
			var sw = new Stopwatch();

			for ( int i = 0 ; i < 10 ; ++i ) {
				sw.Restart();
				client.Query("g");
				Console.WriteLine("t: "+sw.Elapsed.TotalMilliseconds+"ms");
			}
        }

    }
}