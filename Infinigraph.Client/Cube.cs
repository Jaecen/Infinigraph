using OpenTK;
using System.Drawing;

namespace Infinigraph.Client
{
	public class Cube
	{
		public Vector3[] Vertices
		{ get; protected set; }

		public Vector3[] Normals
		{ get; protected set; }

		public Vector2[] Texcoords
		{ get; protected set; }

		public int[] Indices
		{ get; protected set; }

		public int[] Colors
		{ get; protected set; }

		public Cube(Color color)
		{
			Vertices = new Vector3[]
			{
				new Vector3(-0.5f, -0.5f,  0.5f),	//LBT
				new Vector3( 0.5f, -0.5f,  0.5f),	//RBT
				new Vector3( 0.5f,  0.5f,  0.5f),	//RFT
				new Vector3(-0.5f,  0.5f,  0.5f),	//LFT
				new Vector3(-0.5f, -0.5f, -0.5f),	//LBB
				new Vector3( 0.5f, -0.5f, -0.5f),	//RBB
				new Vector3( 0.5f,  0.5f, -0.5f),	//RFB
				new Vector3(-0.5f,  0.5f, -0.5f)	//LFB
			};

			Indices = new int[]
			{
				// front face
				0, 1, 2, 2, 3, 0,
				// top face
				3, 2, 6, 6, 7, 3,
				// back face
				7, 6, 5, 5, 4, 7,
				// left face
				4, 0, 3, 3, 7, 4,
				// bottom face
				0, 1, 5, 5, 4, 0,
				// right face
				1, 5, 6, 6, 2, 1,
			};

			Normals = new Vector3[]
			{
				new Vector3(-1.0f, -1.0f,  1.0f),
				new Vector3( 1.0f, -1.0f,  1.0f),
				new Vector3( 1.0f,  1.0f,  1.0f),
				new Vector3(-1.0f,  1.0f,  1.0f),
				new Vector3(-1.0f, -1.0f, -1.0f),
				new Vector3( 1.0f, -1.0f, -1.0f),
				new Vector3( 1.0f,  1.0f, -1.0f),
				new Vector3(-1.0f,  1.0f, -1.0f),
			};

			Colors = new int[]
			{
				ColorToRgba32(Color.DarkRed),
				ColorToRgba32(Color.DarkRed),
				ColorToRgba32(Color.Gold),
				ColorToRgba32(Color.Gold),
				ColorToRgba32(Color.DarkRed),
				ColorToRgba32(Color.DarkRed),
				ColorToRgba32(Color.Gold),
				ColorToRgba32(Color.Gold),
			};
		}

		public static int ColorToRgba32(Color c)
		{
			return (int)((c.A << 24) | (c.B << 16) | (c.G << 8) | c.R);
		}
	}
}