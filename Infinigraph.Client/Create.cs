using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Infinigraph.Client
{
	static class Create
	{
		public static Drawable Terrain(int edgeSizeInTerrainUnits, int quadsPerTerrainUnit, float oglUnitsPerTerrainUnit, Func<int, int, float> yValue)
		{
			uint indexBase = 0;
			
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var colors = new List<uint>();
			var indices = new List<uint>();

			// Calculate edge size for each quad
			float quadEdgeSizeInOglUnits = (oglUnitsPerTerrainUnit / quadsPerTerrainUnit);

			for(int x = 0; x < edgeSizeInTerrainUnits * quadsPerTerrainUnit - 1; x++)
			{
				var xPlus = x + 1;
				var xQuadEdge = x * quadEdgeSizeInOglUnits;
				var xPlusQuadEdge = xPlus * quadEdgeSizeInOglUnits;

				for(int z = 0; z < edgeSizeInTerrainUnits * quadsPerTerrainUnit - 1; z++)
				{
					var zPlus = z + 1;
					var zQuadEdge = z * quadEdgeSizeInOglUnits;
					var zPlusQuadEdge = zPlus * quadEdgeSizeInOglUnits;

					var v = new[]
					{
						new Vector3(xPlusQuadEdge, yValue(xPlus, zPlus), zPlusQuadEdge),
						new Vector3(xPlusQuadEdge, yValue(xPlus, z), zQuadEdge),
						new Vector3(xQuadEdge, yValue(x, z), zQuadEdge),
						new Vector3(xQuadEdge, yValue(x, zPlus), zPlusQuadEdge),
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
						0xFF880088 | (uint)((byte)0xFF * v[0].Y) << 8,
						0xFF880088 | (uint)((byte)0xFF * v[1].Y) << 8,
						0xFF880088 | (uint)((byte)0xFF * v[2].Y) << 8,
						0xFF880088 | (uint)((byte)0xFF * v[3].Y) << 8,
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

		public static Drawable BlockGrid(int width, int height, Func<int, int, float> yValue)
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
