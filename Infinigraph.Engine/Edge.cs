using System;

namespace Infinigraph.Engine
{
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
