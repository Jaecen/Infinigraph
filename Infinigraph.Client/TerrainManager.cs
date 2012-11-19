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
		// We have three different kinds of units:
		//  - OpenGL units
		//  - Noise range units, which we'll call terrain units
		//  - Chunk indices

		// The chunk size is given in terrain units.
		// There is a conversion factor between terrain and OGL units.
		// Each chunk has a unique corrdinate (a chunk index) derived from a range of scalar coordinate pairs.
		// A chunk index can be represented as a single chunk code.

		// The number of quads per terrain unit is the effective resolution.

		const int ChunkSizeInTerrainUnits = 4;			// Each chunk is 4km by 4km.
		const float OglUnitsPerTerrainUnit = 8;			// Each chunk will be 8x8 OGL units.
		const int QuadsPerTerrainUnit = 20;				// Each chunk will contain 160 quads.
		const int CachedChunkRangeInIndices = 4;		// Only cache chunks within +/- 4 chunk indices (81 sq chunks).
		const int GeneratedChunkRangeInIndices = 2;		// All chunks within +/- 2 chunk indices (25 sq chunks) should be generated.
		const int RenderedChunkRangeInIndices = 1;		// All chunks within +/- 1 chunk indices (9 sq chunks) should be rendered.

		Dictionary<ulong, TerrainChunk> ChunkCache = new Dictionary<ulong, TerrainChunk>();
		LibNoise.Builder.NoiseMapBuilderPlane NoiseBuilder;
		int? LastXIndex = null;
		int? LastZIndex = null;

		public TerrainManager(int seed)
		{
			NoiseBuilder = new LibNoise.Builder.NoiseMapBuilderPlane();
			NoiseBuilder.SourceModule = new LibNoise.Primitive.SimplexPerlin(seed, LibNoise.NoiseQuality.Standard);
			
			// We need to read out one value per quad
			NoiseBuilder.SetSize(ChunkSizeInTerrainUnits * QuadsPerTerrainUnit, ChunkSizeInTerrainUnits * QuadsPerTerrainUnit);
		}

		public void UpdateTerrainCache(Vector3 currentPosition/* View Frustrum */)
		{
			var xIndex = ConvertToIndex(currentPosition.X);
			var zIndex = ConvertToIndex(currentPosition.Z);

			// Only update cache if we've moved to a new chunk
			if(xIndex == LastXIndex && zIndex == LastZIndex)
				return;

			LastXIndex = xIndex;
			LastZIndex = zIndex;

			// Clear out items from the cache that are too far away.
			RemoveIrrelevantTerrainChunks(ChunkCache, xIndex, zIndex);

			// Generate chunks within the generation range
			GenerateImportantTerrainChunks(ChunkCache, xIndex, zIndex);
		}

		private void RemoveIrrelevantTerrainChunks(Dictionary<ulong, TerrainChunk> chunkCache, int xIndex, int zIndex)
		{
			// Get a list of all codes within the cached range
			var cachedCodes = Enumerable.Range(xIndex - CachedChunkRangeInIndices, CachedChunkRangeInIndices * 2)
				.Join(
					Enumerable.Range(zIndex - CachedChunkRangeInIndices, CachedChunkRangeInIndices * 2),
					i => i,
					i => i,
					(x, z) => new
					{
						X = x,
						Z = z,
					},
					new Always())
				.Select(o => CalculateLocationCode(o.X, o.Z));

			// Remove everything for the cache that's not in the range
			foreach(var expiredCodes in chunkCache.Keys.Except(cachedCodes).ToArray())
			{
				chunkCache[expiredCodes].Dispose();
				chunkCache.Remove(expiredCodes);
			}
		}

		private void GenerateImportantTerrainChunks(Dictionary<ulong, TerrainChunk> chunkCache, int xIndex, int zIndex)
		{
			// Generate the location code for all chunks within the generate range
			for(var x = xIndex - GeneratedChunkRangeInIndices; x < xIndex + GeneratedChunkRangeInIndices; x++)
				for(var z = zIndex - GeneratedChunkRangeInIndices; z < zIndex + GeneratedChunkRangeInIndices; z++)
				{
					var code = CalculateLocationCode(x, z);

					// If it's not already in the cache, generate it
					if(!chunkCache.ContainsKey(code))
						chunkCache.Add(code, GenerateTerrainChunk(x, z));
				}
		}

		private TerrainChunk GenerateTerrainChunk(int xIndex, int zIndex)
		{
			// Calculate the area in terrain units that this chunk covers
			float xL = xIndex * ChunkSizeInTerrainUnits;
			float xH = (xIndex + 1) * ChunkSizeInTerrainUnits;
			float zL = zIndex * ChunkSizeInTerrainUnits;
			float zH = (zIndex + 1) * ChunkSizeInTerrainUnits;

			// Arrange them to make the noise generator happy
			float xLowBound = Math.Min(xL, xH);
			float xHighBound = Math.Max(xL, xH);
			float zLowBound = Math.Min(zL, zH);
			float zHighBound = Math.Max(zL, zH);

			// Set up the noise map to be generated over that area
			var noiseMap = new LibNoise.Builder.NoiseMap();
			NoiseBuilder.NoiseMap = noiseMap;
			NoiseBuilder.SetBounds(xLowBound, xHighBound, zLowBound, zHighBound);
			NoiseBuilder.Build();

			// Render the chunk
			return new TerrainChunk(
				xIndex * ChunkSizeInTerrainUnits * OglUnitsPerTerrainUnit,
				zIndex * ChunkSizeInTerrainUnits * OglUnitsPerTerrainUnit,
				Create.Terrain(
					ChunkSizeInTerrainUnits,
					QuadsPerTerrainUnit,
					OglUnitsPerTerrainUnit,
					noiseMap.GetValue));
		}

		private int ConvertToIndex(float value)
		{
			var result = (int)Math.Round(value / ChunkSizeInTerrainUnits / OglUnitsPerTerrainUnit, MidpointRounding.AwayFromZero);

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
			// var visibleTerrainChunks = DetermineVisibleTerrainChunks(ChunkCache, viewFrustrum);

			// Load and render visible terrain chunks

			// Render the 9 closes to the current location
			var xIndex = ConvertToIndex(currentPosition.X);
			var zIndex = ConvertToIndex(currentPosition.Z);

			for(int x = -RenderedChunkRangeInIndices; x <= RenderedChunkRangeInIndices; x++)
				for(int z = -RenderedChunkRangeInIndices; z <= RenderedChunkRangeInIndices; z++)
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
}
