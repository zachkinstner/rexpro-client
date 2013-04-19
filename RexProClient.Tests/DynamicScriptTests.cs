﻿namespace Rexster.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Rexster.Tests.Properties;

    [TestClass]
    public class DynamicScriptTests
    {
        private RexProClient client;

        [TestInitialize]
        public void Initialize()
        {
            client = new RexProClient(Settings.Default.RexProHost, Settings.Default.RexProPort);
        }

        private static string InitScript(string script)
        {
            return
                "g = new TinkerGraph();" +
                "g.addVertex(['name':'V1']);" +
                "g.addVertex(['name':'V2']);" +
                "g.addVertex(['name':'V3']);" +
                script;
        }

        [TestMethod]
        public void QueryScalarValue()
        {
            var script = InitScript("g.V.count()");
            var count = client.Query(script);

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void QuerySingleVertex()
        {
            var script = InitScript("g.V.next()");
            var vertex = client.Query(script);

            Assert.IsNotNull(vertex);
            Assert.IsNotNull(vertex._id);
            Assert.AreEqual(vertex._type, "vertex");
            Assert.IsNotNull(vertex._properties);
            Assert.IsNotNull(vertex._properties.name);
        }

        [TestMethod]
        public void QuerySingleMap()
        {
            var script = InitScript("g.V.next().map()");
            var item = client.Query(script);

            Assert.IsNotNull(item);
            Assert.IsNotNull(item.name);
        }

        [TestMethod]
        public void QueryMultipleVertices()
        {
            var script = InitScript("g.V");
            var vertices = client.Query<dynamic[]>(script);

            Assert.IsNotNull(vertices);
            Assert.AreEqual(3, vertices.Length);
            Assert.IsTrue(vertices.Any(vertex => vertex._properties.name == "V1"));
            Assert.IsTrue(vertices.Any(vertex => vertex._properties.name == "V2"));
            Assert.IsTrue(vertices.Any(vertex => vertex._properties.name == "V3"));
        }

        [TestMethod]
        public void QueryMultipleMaps()
        {
            var script = InitScript("g.V.map()");
            var items = client.Query<dynamic[]>(script);

            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Length);
            Assert.IsTrue(items.Any(item => item.name == "V1"));
            Assert.IsTrue(items.Any(item => item.name == "V2"));
            Assert.IsTrue(items.Any(item => item.name == "V3"));
        }

        [TestMethod]
        public void QueryEdge()
        {
            var script = InitScript("g.addEdge(null,g.v(0),g.v(1),'knows')");
            var edge = client.Query(script);

            Assert.IsNotNull(edge);
            Assert.AreEqual("0", edge._outV);
            Assert.AreEqual("1", edge._inV);
            Assert.AreEqual("knows", edge._label);
        }

        [TestMethod]
        public void DynamicLinq()
        {
            var vertices = client.Query<dynamic[]>("g.V");
            var idQuery =
                from vertex in vertices
                select vertex._id;

            var list = string.Join(",", idQuery);

            Assert.IsFalse(string.IsNullOrEmpty(list));
        }

        [TestMethod]
        public void QueryPath()
        {
            const string script = "g.addEdge(null,g.v(0),g.v(1),'knows');" +
                                  "g.addEdge(null,g.v(0),g.v(2),'knows');" +
                                  "g.addEdge(null,g.v(1),g.v(2),'knows');" +
                                  "g.v(0).out().loop(1){true}{true}.path()";

            var paths = client.Query<IEnumerable<dynamic>>(InitScript(script));
            var pathLengths =
                (from path in paths
                 select new
                 {
                     Path = path,
                     path.Length
                 }).ToArray();

            Assert.IsNotNull(paths);
            Assert.AreEqual(3, pathLengths.Length);
            Assert.AreEqual(2, pathLengths.Min(p => p.Length));
            Assert.AreEqual(3, pathLengths.Max(p => p.Length));
            Assert.AreEqual(2, pathLengths.Count(p => p.Length == 2));
            Assert.AreEqual(1, pathLengths.Count(p => p.Length == 3));
        }
    }
}