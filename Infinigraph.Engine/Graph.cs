using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Infinigraph.Engine
{
	public class Graph : IReadOnlyDictionary<string, Node>
	{
		protected Dictionary<string, Node> Nodes
		{ get; set; }

		public Node this[string index]
		{ get { return Nodes[index]; } }

		public IEnumerable<string> Keys
		{ get { return Nodes.Keys; } }

		public IEnumerable<Node> Values
		{ get { return Nodes.Values; } }

		public int Count
		{ get { return Nodes.Count; } }

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

		public bool ContainsKey(string key)
		{
			return Nodes.ContainsKey(key);
		}

		public bool TryGetValue(string key, out Node value)
		{
			return Nodes.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Nodes.GetEnumerator();
		}

		public IEnumerator<KeyValuePair<string, Node>> GetEnumerator()
		{
			return Nodes.GetEnumerator();
		}
	}
}
