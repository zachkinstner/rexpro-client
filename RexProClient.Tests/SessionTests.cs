namespace Rexster.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rexster.Messages;

    [TestClass]
    public class SessionTests
    {
        private RexProClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = TestClientFactory.CreateClient();
            client.Query("g.V.remove();g.commit()");
        }

        [TestMethod]
        public void OpenCloseSession()
        {
            using (var session = client.StartSession())
            {
                Assert.IsNotNull(session);
            }
        }

        [TestMethod]
        public void UseSessionWithoutGraph()
        {
            using (var session = client.StartSession())
            {
                var expected = client.Query<int>("number = 1 + 2", session: session);
                var actual = client.Query<int>("number", session: session);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void UseSessionWithGraph()
        {
            using (var session = client.StartSession())
            {
                var bindings = new { name = "foo" };
                var request = new ScriptRequest("v = g.addVertex(['name':name])", bindings);
                var expected = client.ExecuteScript<Vertex<TestVertex>>(request, session).Result;
                var actual = client.Query<Vertex<TestVertex>>("v", session: session);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Data.Name, actual.Data.Name);
            }
        }

		[TestMethod]
		public void RollbackSession() {
			const string countQuery = "g.V.count()";

			var expectCount = client.Query<int>(countQuery);
			int actualCount;

			using ( var session = client.StartSession() ) {
				var vertex = client.Query<Vertex<TestVertex>>("g.addVertex()", session: session);
				Assert.IsNotNull(vertex);

				actualCount = client.Query<int>(countQuery, session: session);
				Assert.AreEqual(expectCount+1, actualCount, "A vertex should be added.");

				var rollback = client.Query<int>("g.rollback();1", session: session);
				Assert.AreEqual(1, rollback);

				actualCount = client.Query<int>(countQuery, session: session);
				Assert.AreEqual(expectCount, actualCount, "The new vertex persists after rollback.");
			}

			actualCount = client.Query<int>(countQuery);
			Assert.AreEqual(expectCount, actualCount, "The new vertex persists outside the session.");
		}

		[TestMethod]
		public void RollbackSessionSingleScript() {
			const string testQuery = "a=g.V.count();"+
				"g.addVertex();"+
				"b=g.V.count();"+
				"g.rollback();"+
				"c=g.V.count();"+
				"[a,b,c];";

			var counts = client.Query<int[]>(testQuery);
			Assert.AreEqual(3, counts.Length);
			Assert.AreEqual(counts[0]+1, counts[1], "The new vertex was not created.");
			Assert.AreEqual(counts[0], counts[2], "The new vertex persists after rollback.");
		}

    }
}