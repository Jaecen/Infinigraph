using System;
using System.Collections.Generic;

namespace Infinigraph.Engine
{
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
}
