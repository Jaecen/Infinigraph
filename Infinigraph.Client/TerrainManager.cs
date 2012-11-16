using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Infinigraph.Client
{
	class TerrainManager
	{
		Dictionary<ulong, TerrainChunk> ChunkCache = new Dictionary<ulong, TerrainChunk>();
		int ChunkSize;
		LibNoise.Builder.NoiseMapBuilderPlane NoiseBuilder;

		public TerrainManager(int seed, int chunkSize)
		{
			ChunkSize = chunkSize;
			NoiseBuilder = new LibNoise.Builder.NoiseMapBuilderPlane();
			NoiseBuilder.SourceModule = new LibNoise.Primitive.SimplexPerlin(10, LibNoise.NoiseQuality.Standard);
			NoiseBuilder.SetSize(ChunkSize, ChunkSize);
		}

		/*
			GenerateTerrain();
			TerrainManager.LoadBuffers();

		 * 
			int gridWidth = 10;
			int gridHeight = 10;
			int quadsPerUnit = 10;

			var noiseMap = new LibNoise.Builder.NoiseMap();
			var builder = new LibNoise.Builder.NoiseMapBuilderPlane();
			builder.SourceModule = new LibNoise.Primitive.SimplexPerlin(10, LibNoise.NoiseQuality.Standard);
			builder.NoiseMap = noiseMap;
			builder.SetSize(gridWidth * quadsPerUnit, gridHeight * quadsPerUnit);
			builder.SetBounds(0, gridWidth, 0, gridWidth);
			builder.Build();

			TerrainManager = Create.Terrain(
				gridWidth,
				gridHeight,
				quadsPerUnit,
				noiseMap.GetValue);
		
		 * 
			if(TerrainManager != null)
				TerrainManager.DeleteBuffers();

		 * 
			TerrainManager.Draw();
		*/

		public void UpdateTerrainCache(Vector3 currentPosition/* View Frustrum */)
		{
			// Remove terrain chunks from cache that are no longer relevant.
			RemoveIrrelevantTerrainChunks(ChunkCache, currentPosition);

			// Generate important missing terrain chunks.
			GenerateImportantTerrainChunks(ChunkCache, currentPosition);
		}

		private void RemoveIrrelevantTerrainChunks(Dictionary<ulong, TerrainChunk> chunkCache, Vector3 currentPosition)
		{
			// All chunks more the +/-4 of current are irrelevant
			const int relevanceDistance = 4;
			var xIndex = ConvertToIndex(currentPosition.X);
			var zIndex = ConvertToIndex(currentPosition.Z);

			var relevantCodes = Enumerable.Range(xIndex - relevanceDistance, relevanceDistance * 2)
				.Join(
					Enumerable.Range(zIndex - relevanceDistance, relevanceDistance * 2),
					i => i,
					i => i,
					(x, z) => new
					{
						X = x,
						Z = z,
					},
					new Always())
				.Select(o => CalculateLocationCode(o.X, o.Z));

			foreach(var irrelevantCode in chunkCache.Keys.Except(relevantCodes).ToArray())
			{
				chunkCache[irrelevantCode].Dispose();
				chunkCache.Remove(irrelevantCode);
			}
		}

		private void GenerateImportantTerrainChunks(Dictionary<ulong, TerrainChunk> chunkCache, Vector3 currentPosition)
		{
			// All chunks within +/-2 of current are important
			const int relevanceDistance = 2;
			var xIndex = ConvertToIndex(currentPosition.X);
			var zIndex = ConvertToIndex(currentPosition.Z);

			for(var x = xIndex - relevanceDistance; x < xIndex + relevanceDistance; x++)
				for(var z = zIndex - relevanceDistance; z < zIndex + relevanceDistance; z++)
				{
					var code = CalculateLocationCode(x, z);
					if(!chunkCache.ContainsKey(code))
						chunkCache.Add(code, GenerateTerrainChunk(x, z));
				}
		}

		private TerrainChunk GenerateTerrainChunk(int xIndex, int zIndex)
		{
			int xL = xIndex * ChunkSize;
			int xH = xIndex + 1 * ChunkSize;
			int zL = zIndex * ChunkSize;
			int zH = zIndex + 1 * ChunkSize;

			int xLowBound = Math.Min(xL, xH);
			int xHighBound = Math.Max(xL, xH);
			int zLowBound = Math.Min(zL, zH);
			int zHighBound = Math.Max(zL, zH);

			var noiseMap = new LibNoise.Builder.NoiseMap();
			NoiseBuilder.NoiseMap = noiseMap;
			NoiseBuilder.SetBounds(xLowBound, xHighBound, zLowBound, zHighBound);
			NoiseBuilder.Build();

			uint color = 0xFF000000 | (uint)(128 + (xIndex * 4)) << 16 | (uint)(128 + (zIndex * 4));

			return new TerrainChunk(
				xIndex * ChunkSize,
				zIndex * ChunkSize,
				Create.Terrain(
					ChunkSize,
					ChunkSize,
					1,
					noiseMap.GetValue,
					color));
		}

		private int ConvertToIndex(float value)
		{
			var result = (int)Math.Round(value / ChunkSize, MidpointRounding.AwayFromZero);

			if(result == 0)
				return 1;

			return result;
		}

		private ulong CalculateLocationCode(int xIndex, int zIndex)
		{
			return (ulong)xIndex << 32 | (uint)zIndex;
		}

		public void DrawTerrain(Vector3 currentPosition/* View Frustrum */)
		{
			// Determine which terrain chunks are visible
			//var visibleTerrainChunks = DetermineVisibleTerrainChunks(ChunkCache, viewFrustrum);

			// Load and render visible terrain chunks

			// Render the 9 closes to the current location
			var xIndex = ConvertToIndex(currentPosition.X);
			var zIndex = ConvertToIndex(currentPosition.Z);
			for(int x = -1; x <= 1; x++)
				for(int z = -1; z <= 1; z++)
					ChunkCache[CalculateLocationCode(xIndex + x, zIndex + z)].Draw();
		}
	}

	class Always : IEqualityComparer<int>
	{
		public bool Equals(int x, int y)
		{
			return true;
		}

		public int GetHashCode(int obj)
		{
			return obj;
		}
	}

	class TerrainChunk
	{
		Drawable Drawable;
		bool Loaded = false;
		int X;
		int Y;

		public TerrainChunk(int x, int y, Drawable drawable)
		{
			X = x;
			Y = y;
			Drawable = drawable;
		}

		public void Draw()
		{
			if(!Loaded)
			{
				Drawable.LoadBuffers();
				Loaded = true;
			}

			GL.PushMatrix();
			GL.Translate(X, 0, Y);
			Drawable.Draw();
			GL.PopMatrix();
		}

		public void Dispose()
		{
			Drawable.DeleteBuffers();
		}
	}
}
