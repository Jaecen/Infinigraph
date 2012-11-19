using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Infinigraph.Client
{
	class TerrainChunk
	{
		Drawable Drawable;
		bool Loaded = false;
		float X;
		float Z;

		public TerrainChunk(float x, float z, Drawable drawable)
		{
			X = x;
			Z = z;
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
			GL.Translate(X, 0, Z);
			Drawable.Draw();
			GL.PopMatrix();
		}

		public void Dispose()
		{
			Drawable.DeleteBuffers();
		}
	}
}
