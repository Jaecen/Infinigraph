using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace Infinigraph.Client
{
	class ClientWindow : GameWindow
	{
		const string VertexShaderSource = @"
			void main()
			{
				gl_FrontColor = gl_Color;
				gl_Position = ftransform();
			}";

		const string FragmentShaderSource = @"
			void main()
			{
				gl_FragColor = gl_Color;
			}";

		int VertexShader;
		int FragmentShader;
		int ShaderProgram;
		int VertexBuffer;
		int ColorBuffer;
		int ElementBuffer;

		Cube cube = new Cube();

		public ClientWindow()
			: base(800, 600)
		{ }

		// This is the place to load resources that change little during the lifetime of the GameWindow. 
		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(Color.MidnightBlue);
			GL.Enable(EnableCap.DepthTest);

			VertexBuffer = CreateBuffer(BufferTarget.ArrayBuffer, cube.Vertices.Length * 3 * sizeof(float), cube.Vertices);
			ColorBuffer = CreateBuffer(BufferTarget.ArrayBuffer, cube.Colors.Length * sizeof(int), cube.Colors);
			ElementBuffer = CreateBuffer(BufferTarget.ElementArrayBuffer, cube.Indices.Length * sizeof(int), cube.Indices);

			VertexShader = CreateShader(VertexShaderSource, ShaderType.VertexShader);
			FragmentShader = CreateShader(FragmentShaderSource, ShaderType.FragmentShader);
			ShaderProgram = CreateShaderProgram(VertexShader, FragmentShader);
		}

		int CreateShader(string shaderSource, ShaderType shaderType)
		{
			int shader = GL.CreateShader(shaderType);

			GL.ShaderSource(shader, shaderSource);
			GL.CompileShader(shader);

			int status_code;
			GL.GetShader(shader, ShaderParameter.CompileStatus, out status_code);

			if(status_code != 1)
			{
				string log;
				GL.GetShaderInfoLog(shader, out log);
				throw new ApplicationException(log);
			}

			return shader;
		}

		int CreateShaderProgram(int vertexShader, int fragmentShader)
		{
			int program = GL.CreateProgram();
		
			GL.AttachShader(program, fragmentShader);
			GL.AttachShader(program, vertexShader);

			GL.LinkProgram(program);
			GL.UseProgram(program);
			
			return program;
		}

		int CreateBuffer<T>(BufferTarget bufferTarget, int size, T[] data)
			where T: struct
		{
			int buffer;

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
		
		protected override void OnUnload(EventArgs e)
		{
			if(ShaderProgram != 0)
				GL.DeleteProgram(ShaderProgram);

			if(FragmentShader != 0)
				GL.DeleteShader(FragmentShader);

			if(VertexShader != 0)
				GL.DeleteShader(VertexShader);

			if(VertexBuffer != 0)
				GL.DeleteBuffers(1, ref VertexBuffer);

			if(ColorBuffer != 0)
				GL.DeleteBuffers(1, ref ColorBuffer);

			if(ElementBuffer != 0)
				GL.DeleteBuffers(1, ref ElementBuffer);
		}

		// Called when the user resizes the window. You want the OpenGL viewport to match the window. This is the place to do it!
		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);

			float aspect_ratio = Width / (float)Height;
			Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref perpective);
		}

		// Prepares the next frame for rendering. Place your control logic here. 
		// This is the place to respond to user input, update object positions etc.
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if(Keyboard[OpenTK.Input.Key.Escape])
				this.Exit();

			if(Keyboard[OpenTK.Input.Key.F11])
				WindowState = WindowState == OpenTK.WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
		}

		// Place your rendering code here.
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Matrix4 lookat = Matrix4.LookAt(0, 5, 5, 0, 0, 0, 0, 1, 0);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);

			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.ColorArray);

			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
			GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);
			GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBuffer);
			GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, IntPtr.Zero);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);

			GL.DrawElements(BeginMode.Triangles, cube.Indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

			GL.DisableClientState(ArrayCap.VertexArray);
			GL.DisableClientState(ArrayCap.ColorArray);

			SwapBuffers();
		}
	}
}