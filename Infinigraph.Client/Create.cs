using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Infinigraph.Client
{
	static class Create
	{
		public static Drawable Terrain()
		{
			int width = 10;
			int height = 10;

			int gridFactor = 10;
			
			var noiseMap = new LibNoise.Builder.NoiseMap();
			var builder = new LibNoise.Builder.NoiseMapBuilderPlane();
			builder.SourceModule = new LibNoise.Primitive.SimplexPerlin(10, LibNoise.NoiseQuality.Standard);
			builder.NoiseMap = noiseMap;
			builder.SetSize(width * gridFactor, height * gridFactor);
			builder.SetBounds(0, width, 0, height);
			builder.Build();

			uint indexBase = 0;
			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<uint> colors = new List<uint>();
			List<uint> indices = new List<uint>();

			// Take each strip of points and render a quad for each pair of pairs.
			for(int x = 0; x < width * gridFactor - 1; x++)
			{
				for(int z = 0; z < height * gridFactor - 1; z++)
				{
					var v = new[]
					{
						new Vector3(x + 1, noiseMap.GetValue(x + 1, z + 1), z + 1),
						new Vector3(x + 1, noiseMap.GetValue(x + 1, z + 0), z + 0),
						new Vector3(x + 0, noiseMap.GetValue(x + 0, z + 0), z + 0),
						new Vector3(x + 0, noiseMap.GetValue(x + 0, z + 1), z + 1),
					};
					vertices.AddRange(v);

					normals.AddRange(new[]
					{
						Vector3.Normalize(Vector3.Cross(v[1] - v[0], v[2] - v[0])),
						Vector3.Normalize(Vector3.Cross(v[1] - v[0], v[2] - v[0])),
						Vector3.Normalize(Vector3.Cross(v[1] - v[0], v[2] - v[0])),
						Vector3.Normalize(Vector3.Cross(v[3] - v[2], v[0] - v[2])),
					});

					colors.AddRange(new[]
					{
						0xFF888888,
						0xFF888888,
						0xFF888888,
						0xFF888888,
					});

					indices.AddRange(new uint[] 
					{
						indexBase + 0, indexBase + 1, indexBase + 2,
						indexBase + 2, indexBase + 3, indexBase + 0,
					});

					indexBase += (uint)v.Length;
				}
			}

			return new Drawable(vertices.ToArray(), normals.ToArray(), colors.ToArray(), indices.ToArray());

		}

		public static Drawable Grid(int width, int height, Func<int, int, float> yValue)
		{
			uint indexBase = 0;
			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<uint> colors = new List<uint>();
			List<uint> indices = new List<uint>();

			float y;

			for(int x = 0; x < width; x++)
			{
				for(int z = 0; z < height; z++)
				{
					y = yValue(x, z);

					vertices.AddRange(new[]
					{
						new Vector3(x + -0.5f, y + -0.5f, z +  0.5f),	//LBT
						new Vector3(x +  0.5f, y + -0.5f, z +  0.5f),	//RBT
						new Vector3(x +  0.5f, y +  0.5f, z +  0.5f),	//RFT
						new Vector3(x + -0.5f, y +  0.5f, z +  0.5f),	//LFT
						new Vector3(x + -0.5f, y + -0.5f, z + -0.5f),	//LBB
						new Vector3(x +  0.5f, y + -0.5f, z + -0.5f),	//RBB
						new Vector3(x +  0.5f, y +  0.5f, z + -0.5f),	//RFB
						new Vector3(x + -0.5f, y +  0.5f, z + -0.5f)	//LFB
					});

					normals.AddRange(new[]
					{
						new Vector3(-1.0f, -1.0f,  1.0f),
						new Vector3( 1.0f, -1.0f,  1.0f),
						new Vector3( 1.0f,  1.0f,  1.0f),
						new Vector3(-1.0f,  1.0f,  1.0f),
						new Vector3(-1.0f, -1.0f, -1.0f),
						new Vector3( 1.0f, -1.0f, -1.0f),
						new Vector3( 1.0f,  1.0f, -1.0f),
						new Vector3(-1.0f,  1.0f, -1.0f),
					});

					colors.AddRange(new[]
					{
						0xFF888888,
						0xFF888888,
						0xFF888888,
						0xFF888888,
						0xFF888888,
						0xFF888888,
						0xFF888888,
						0xFF888888,
					});

					indices.AddRange(new uint[] 
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
					}.Select(i => indexBase + i));

					indexBase += 8;
				}
			}

			return new Drawable(vertices.ToArray(), normals.ToArray(), colors.ToArray(), indices.ToArray());
		}
	}
}
