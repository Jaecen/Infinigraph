using System;
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
}
