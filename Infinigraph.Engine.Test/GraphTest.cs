using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Infinigraph.Engine.Test
{
	public class GraphTest
	{
		[Fact]
		public void Should_Add_Nodes()
		{
			var graph0 = new Graph(Enumerable.Empty<KeyValuePair<string, Node>>());
			var graph1 = graph0.AddNode("one");
			var graph2 = graph1.AddNode("two");

			Assert.Empty(graph0);
		
			Assert.Equal(1, graph1.Count());
			Assert.True(graph1.ContainsKey("one"));
			Assert.Equal("one", graph1["one"].Id);

			Assert.Equal(2, graph2.Count());
			Assert.True(graph2.ContainsKey("one"));
			Assert.True(graph2.ContainsKey("two"));
			Assert.Equal("one", graph2["one"].Id);
			Assert.Equal("two", graph2["two"].Id);
		}

		[Fact]
		public void Should_Add_Edges()
		{
			var graph0 = new Graph(new[] { 
				new Node("one", Enumerable.Empty<Edge>()),
				new Node("two", Enumerable.Empty<Edge>()),
				new Node("three", Enumerable.Empty<Edge>()),
			}.ToDictionary(n => n.Id));

			var graph1 = graph0.AddEdge("one", "two", 1, 1);
			var graph2 = graph1.AddEdge("two", "three", 2, 2);

			Assert.Equal(0, graph0.Sum(kvp => kvp.Value.Edges.Count()));

			Assert.Equal(1, graph1["one"].Edges.Count());
			Assert.Equal(0, graph1["two"].Edges.Count());
			Assert.Equal(0, graph1["three"].Edges.Count());
			Assert.Equal(1, graph1.Sum(kvp => kvp.Value.Edges.Sum(e => e.Length)));
			Assert.Equal(1, graph1.Sum(kvp => kvp.Value.Edges.Sum(e => e.Length)));

			Assert.Equal(1, graph2["one"].Edges.Count());
			Assert.Equal(1, graph2["two"].Edges.Count());
			Assert.Equal(0, graph2["three"].Edges.Count());
			Assert.Equal(3, graph2.Sum(kvp => kvp.Value.Edges.Sum(e => e.Length)));
			Assert.Equal(3, graph2.Sum(kvp => kvp.Value.Edges.Sum(e => e.Length)));
		}

		[Fact]
		public void Should_Load_Nodes()
		{
			var data =
				@"<graph>
					<nodes>
						<node id='one' />
						<node id='two' />
						<node id='three' />
					</nodes>
				</graph>";

			XmlGraphLoader graphLoader = new XmlGraphLoader();
			using(var source = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(data)))
			{
				var graph = graphLoader.LoadGraph(source);

				Assert.True(graph.ContainsKey("one"));
				Assert.True(graph.ContainsKey("two"));
				Assert.True(graph.ContainsKey("three"));
			}
		}

		[Fact]
		public void Should_Load_Edges()
		{
			var data =
				@"<graph>
					<nodes>
						<node id='one' />
						<node id='two' />
						<node id='three' />
					</nodes>
					<edges>
						<edge start='one' end='two' length='3' rate='1' />
						<edge start='two' end='three' length='5' rate='2' />
						<edge start='three' end='one' length='4' rate='3' />
						<edge start='one' end='three' length='4' rate='4' />
					</edges>
				</graph>";

			XmlGraphLoader graphLoader = new XmlGraphLoader();
			using(var source = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(data)))
			{
				var graph = graphLoader.LoadGraph(source);

				Assert.Equal(2, graph["one"].Edges.Count());
				Assert.Equal(1, graph["two"].Edges.Count());
				Assert.Equal(1, graph["three"].Edges.Count());
			}
		}
	}
}
