using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinigraph.Client
{
	class Grid
	{
		public Vector3[] Vertices
		{ get; protected set; }
		
		private uint _VertexBuffer;
		public uint VertexBuffer
		{ 
			get { return _VertexBuffer; } 
			protected set{ _VertexBuffer = value; }  
		}

		public Vector3[] Normals
		{ get; protected set; }

		private uint _NormalBuffer;
		public uint NormalBuffer
		{ 
			get { return _NormalBuffer; } 
			protected set{ _NormalBuffer = value; }  
		}

		public uint[] Colors
		{ get; protected set; }

		private uint _ColorBuffer;
		public uint ColorBuffer
		{ 
			get { return _ColorBuffer; } 
			protected set{ _ColorBuffer = value; }  
		}

		public uint[] Indices
		{ get; protected set; }

		private uint _IndexBuffer;
		public uint IndexBuffer
		{ 
			get { return _IndexBuffer; } 
			protected set{ _IndexBuffer = value; }  
		}

		public Grid(int width, int height, Func<int, int, float> yValue)
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

			Vertices = vertices.ToArray();
			Normals = normals.ToArray();
			Colors = colors.ToArray();
			Indices = indices.ToArray();
		}

		public void LoadBuffers()
		{
			VertexBuffer = CreateBuffer(BufferTarget.ArrayBuffer, Vertices.Length * 3 * sizeof(float), Vertices);
			NormalBuffer = CreateBuffer(BufferTarget.ArrayBuffer, Normals.Length * 3 * sizeof(float), Normals);
			ColorBuffer = CreateBuffer(BufferTarget.ArrayBuffer, Colors.Length * sizeof(uint), Colors);
			IndexBuffer = CreateBuffer(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices);
		}

		public void BindBuffers()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
			GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBuffer);
			GL.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
			GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, IntPtr.Zero);
			
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);
		}

		public void DrawGrid()
		{
			GL.DrawElements(BeginMode.Triangles, Indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
		}

		public void DeleteBuffers()
		{
			if(VertexBuffer != 0)
				GL.DeleteBuffers(1, ref _VertexBuffer);

			if(NormalBuffer != 0)
				GL.DeleteBuffers(1, ref _NormalBuffer);

			if(ColorBuffer != 0)
				GL.DeleteBuffers(1, ref _ColorBuffer);

			if(IndexBuffer != 0)
				GL.DeleteBuffers(1, ref _IndexBuffer);
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
