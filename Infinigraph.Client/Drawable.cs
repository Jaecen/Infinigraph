using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Infinigraph.Client
{
	class Drawable
	{
		private uint VertexBuffer;
		private uint NormalBuffer;
		private uint ColorBuffer;
		private uint IndexBuffer;
		private int IndexCount;
		private Action BufferLoader;

		public Drawable(Vector3[] vertices, Vector3[] normals, uint[] colors, uint[] indices)
		{
			BufferLoader = () =>
			{
				VertexBuffer = CreateBuffer(BufferTarget.ArrayBuffer, vertices.Length * 3 * sizeof(float), vertices);
				NormalBuffer = CreateBuffer(BufferTarget.ArrayBuffer, normals.Length * 3 * sizeof(float), normals);
				ColorBuffer = CreateBuffer(BufferTarget.ArrayBuffer, colors.Length * sizeof(uint), colors);
				IndexBuffer = CreateBuffer(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices);

				IndexCount = indices.Length;
			};
		}

		public void LoadBuffers()
		{
			BufferLoader();
		}

		public void Draw()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
			GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBuffer);
			GL.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
			GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);

			GL.DrawElements(BeginMode.Triangles, IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
		}

		public void DeleteBuffers()
		{
			if(VertexBuffer != 0)
				GL.DeleteBuffers(1, ref VertexBuffer);

			if(NormalBuffer != 0)
				GL.DeleteBuffers(1, ref NormalBuffer);

			if(ColorBuffer != 0)
				GL.DeleteBuffers(1, ref ColorBuffer);

			if(IndexBuffer != 0)
				GL.DeleteBuffers(1, ref IndexBuffer);
		}

		uint CreateBuffer<T>(BufferTarget bufferTarget, int size, T[] data)
			where T : struct
		{
			uint buffer;

			// Upload the buffer
			GL.GenBuffers(1, out buffer);
			GL.BindBuffer(bufferTarget, buffer);
			GL.BufferData(bufferTarget, (IntPtr)(size), data, BufferUsageHint.StaticDraw);

			// Verify
			int allocatedSize;
			GL.GetBufferParameter(bufferTarget, BufferParameterName.BufferSize, out allocatedSize);
			if(allocatedSize != size)
				throw new ApplicationException(String.Format("Problem uploading vertex buffer to VBO (vertices). Tried to upload {0} bytes, uploaded {1}.", size, allocatedSize));

			return buffer;
		}
	}
}