using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinigraph.Client
{
	class Program
	{
		static void Main(string[] args)
		{
			using(var clientWindow = new ClientWindow())
			{
				clientWindow.Title = "Infinigraph Client";
				clientWindow.Run(30);
			}
		}
	}
}
