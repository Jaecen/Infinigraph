using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Infinigraph.Engine
{
	public class XmlGraphLoader
	{
		public Graph LoadGraph(Stream source)
		{
			var sourceDoc = XDocument.Load(source);

			var nodes = sourceDoc.Root
				.Elements("nodes")
				.Elements("node")
				.Select(e => e.Attributes("id")
					.Select(a => a.Value)
					.FirstOrDefault())
				.ToDictionary(s => s, s => new Node(s, Enumerable.Empty<Edge>()));

			var edges = sourceDoc.Root
				.Elements("edges")
				.Elements("edge")
				.Select(e => new
				{
					Start = e.Attributes("start").Select(a => a.Value).FirstOrDefault(),
					End = e.Attributes("end").Select(a => a.Value).FirstOrDefault(),
					Length = e.Attributes("length").Select(a => Decimal.Parse(a.Value)).FirstOrDefault(),
					Rate = e.Attributes("rate").Select(a => Decimal.Parse(a.Value)).FirstOrDefault(),
				});

			var graph = edges.Aggregate(new Graph(nodes), (g, e) => g.AddEdge(e.Start, e.End, e.Length, e.Rate));

			return graph;
		}
	}

	public class Graph : IReadOnlyDictionary<string, Node>
	{
		protected Dictionary<string, Node> Nodes
		{ get; set; }

		public Node this[string index]
		{ get { return Nodes[index]; } }

		public Graph(IEnumerable<KeyValuePair<string, Node>> nodes)
		{
			if(nodes == null)
				throw new ArgumentNullException("nodes");

			Nodes = nodes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public Graph(Graph graph, IEnumerable<KeyValuePair<string, Node>> nodes = null)
			: this(nodes ?? graph.Nodes)
		{ }

		public Graph AddNode(string id)
		{
			if(id == null)
				throw new ArgumentNullException("id");

			return new Graph(this, Nodes.Concat(new[] { new KeyValuePair<string, Node>(id, new Node(id, Enumerable.Empty<Edge>())) }));
		}

		public Graph AddEdge(string startId, string endId, decimal length, decimal rate)
		{
			if(startId == null)
				throw new ArgumentNullException("startId");

			if(!Nodes.ContainsKey(startId))
				throw new ArgumentException(String.Format("No nodes with the ID \"{0}\" exist.", startId), startId);

			if(endId == null)
				throw new ArgumentNullException("endId");

			if(!Nodes.ContainsKey(endId))
				throw new ArgumentException(String.Format("No nodes with the ID \"{0}\" exist.", endId), endId);

			var start = new KeyValuePair<string, Node>(
				startId,
				new Node(
					Nodes[startId],
					edges: Nodes[startId]
						.Edges
						.Concat(new[] 
						{ 
							new Edge(length, rate, Nodes[endId])
						})
				)
			);

			return new Graph(Nodes
				.Where(kvp => kvp.Key != startId)
				.Concat(new[] { start })
			);
		}

		public IEnumerator<KeyValuePair<string, Node>> GetEnumerator()
		{
			return Nodes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Nodes.GetEnumerator();
		}

		public bool ContainsKey(string key)
		{
			return Nodes.ContainsKey(key);
		}

		public IEnumerable<string> Keys
		{ get { return Nodes.Keys; } }

		public bool TryGetValue(string key, out Node value)
		{
			return Nodes.TryGetValue(key, out value);
		}

		public IEnumerable<Node> Values
		{ get { return Nodes.Values; } }

		public int Count
		{ get { return Nodes.Count; } }
	}

	public class Node
	{
		public string Id
		{ get; protected set; }

		public IEnumerable<Edge> Edges
		{ get; protected set; }

		public Node(string id, IEnumerable<Edge> edges)
		{
			if(id == null)
				throw new ArgumentNullException("id");

			if(edges == null)
				throw new ArgumentNullException("edges");

			Id = id;
			Edges = edges;
		}

		public Node(Node node, string id = null, IEnumerable<Edge> edges = null)
			: this(id ?? node.Id, edges ?? node.Edges)
		{ }
	}

	public class Edge
	{
		public decimal Length
		{ get; protected set; }

		public decimal Rate
		{ get; protected set; }

		public Node Target
		{ get; protected set; }

		public Edge(decimal length, decimal rate, Node target)
		{
			if(length < 0)
				throw new ArgumentException("Value must be greater than or equal to zero", "length");

			if(rate < 0)
				throw new ArgumentException("Value must be greater than or equal to zero", "rate");

			if(target == null)
				throw new ArgumentNullException("target");

			Length = length;
			Rate = rate;
			Target = target;
		}

		public Edge(Edge edge, decimal? length = null, decimal? rate = null, Node target = null)
			: this(length ?? edge.Length, rate ?? edge.Rate, target ?? edge.Target)
		{ }
	}
}
