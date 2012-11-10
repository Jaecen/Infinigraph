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
				vec3 normal, lightDir;
				vec4 diffuse, specular, ambient, globalAmbient;
				float NdotL, NdotHV;

				/* first transform the normal into eye space and normalize the result */
				normal = normalize(gl_NormalMatrix * gl_Normal);
				
				/* now normalize the light's direction. Note that according to the
				OpenGL specification, the light is stored in eye space. Also since
				we're talking about a directional light, the position field is actually
				direction */
				lightDir = normalize(vec3(gl_LightSource[0].position));

				/* compute the cos of the angle between the normal and lights direction.
				The light is directional so the direction is constant for every vertex.
				Since these two are normalized the cosine is the dot product. We also
				need to clamp the result to the [0,1] range. */
				NdotL = max(dot(normal, lightDir), 0.0);

				/* compute the specular term if NdotL is  larger than zero */
				if (NdotL > 0.0) {
					// normalize the half-vector, and then compute the
					// cosine (dot product) with the normal
					NdotHV = max(dot(normal, gl_LightSource[0].halfVector.xyz),0.0);
					specular = gl_FrontMaterial.specular * gl_LightSource[0].specular *
					pow(NdotHV,gl_FrontMaterial.shininess);
				}

				/* Compute the diffuse term */
				diffuse = gl_FrontMaterial.diffuse * gl_LightSource[0].diffuse;

				/* Compute the ambient and globalAmbient terms */
				ambient = gl_FrontMaterial.ambient * gl_LightSource[0].ambient;
				globalAmbient = gl_LightModel.ambient * gl_FrontMaterial.ambient;

				gl_FrontColor =  NdotL * diffuse + globalAmbient + ambient;
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
		int NormalBuffer;
		int GrayColorBuffer;
		int BlackColorBuffer;
		int ElementBuffer;

		Cube grayCube = new Cube(Color.Gray);

		const float cameraPanRate = 10f;
		const float cameraZoomRate = 20f;
		const float cameraRotateRate = (float)(40 * Math.PI);
		Vector3 CameraEye = new Vector3(0, 15, 5);
		Vector3 CameraTarget = new Vector3(0, 0, 0);
		Vector3 CameraUp = new Vector3(0, 1, 0);
		float CameraRotate = 0;

		uint[] BlackColors = new uint[]
		{
			0x00000000,
			0x00000000,
			0x00000000,
			0x00000000,
			0x00000000,
			0x00000000,
			0x00000000,
			0x00000000,
		};

		uint[] GrayColors = new uint[]
		{
			0xFF888888,
			0xFF888888,
			0xFF888888,
			0xFF888888,
			0xFF888888,
			0xFF888888,
			0xFF888888,
			0xFF888888,
		};

		const int GridWidth = 50;
		const int GridHeight = 50;
		float[,] GridY = new float[GridHeight, GridWidth];

		public ClientWindow()
			: base(800, 600)
		{ }

		// This is the place to load resources that change little during the lifetime of the GameWindow. 
		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(Color.MidnightBlue);
			GL.Enable(EnableCap.DepthTest);

			VertexBuffer = CreateBuffer(BufferTarget.ArrayBuffer, grayCube.Vertices.Length * 3 * sizeof(float), grayCube.Vertices);
			NormalBuffer = CreateBuffer(BufferTarget.ArrayBuffer, grayCube.Normals.Length * 3 * sizeof(float), grayCube.Normals);
			GrayColorBuffer = CreateBuffer(BufferTarget.ArrayBuffer, GrayColors.Length * sizeof(uint), GrayColors);
			BlackColorBuffer = CreateBuffer(BufferTarget.ArrayBuffer, BlackColors.Length * sizeof(uint), BlackColors);
			ElementBuffer = CreateBuffer(BufferTarget.ElementArrayBuffer, grayCube.Indices.Length * sizeof(int), grayCube.Indices);

			VertexShader = CreateShader(VertexShaderSource, ShaderType.VertexShader);
			FragmentShader = CreateShader(FragmentShaderSource, ShaderType.FragmentShader);
			ShaderProgram = CreateShaderProgram(VertexShader, FragmentShader);

			for(int x = 0; x < GridWidth; x++)
				for(int z = 0; z < GridHeight; z++)
					GridY[x, z] = Noise.Generate(x / (float)GridWidth * 5, z / (float)GridHeight * 5) * 2;
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

			if(GrayColorBuffer != 0)
				GL.DeleteBuffers(1, ref GrayColorBuffer);

			if(ElementBuffer != 0)
				GL.DeleteBuffers(1, ref ElementBuffer);
		}

		// Called when the user resizes the window. You want the OpenGL viewport to match the window. This is the place to do it!
		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);

			float aspect_ratio = Width / (float)Height;
			Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 512);
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

			if(Keyboard[OpenTK.Input.Key.Left])
			{
				CameraEye.X -= (float)(e.Time * cameraPanRate);
				CameraTarget.X -= (float)(e.Time * cameraPanRate);
			}

			if(Keyboard[OpenTK.Input.Key.Right])
			{
				CameraEye.X += (float)(e.Time * cameraPanRate);
				CameraTarget.X += (float)(e.Time * cameraPanRate);
			}

			if(Keyboard[OpenTK.Input.Key.Up])
			{
				CameraEye.Z -= (float)(e.Time * cameraPanRate);
				CameraTarget.Z -= (float)(e.Time * cameraPanRate);
			}

			if(Keyboard[OpenTK.Input.Key.Down])
			{
				CameraEye.Z += (float)(e.Time * cameraPanRate);
				CameraTarget.Z += (float)(e.Time * cameraPanRate);
			}

			if(Keyboard[OpenTK.Input.Key.A] && CameraEye.Y > 1)
			{
				CameraEye.Y -= (float)(e.Time * cameraZoomRate);
			}

			if(Keyboard[OpenTK.Input.Key.Z])
			{
				CameraEye.Y += (float)(e.Time * cameraZoomRate);
			}

			if(Keyboard[OpenTK.Input.Key.Q])
			{
				CameraRotate += (float)(e.Time * cameraRotateRate);
			}

			if(Keyboard[OpenTK.Input.Key.E])
			{
				CameraRotate -= (float)(e.Time * cameraRotateRate);
			}
		}

		// Place your rendering code here.
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Matrix4 lookat = Matrix4.LookAt(CameraEye, CameraTarget, CameraUp);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			GL.Rotate(CameraRotate, CameraUp);

			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.NormalArray);
			GL.EnableClientState(ArrayCap.ColorArray);

			// Draw gray cubes
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
			GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBuffer);
			GL.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, GrayColorBuffer);
			GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, IntPtr.Zero);
			
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);

			GL.Translate(-(GridWidth / 2), 0, -(GridHeight / 2));

			float y = GridY[0, 0];
			float lastY = y;
			GL.Translate(0, y, 0);

			for(int x = 0; x < GridWidth; x++)
			{
				for(int z = 0; z < GridHeight; z++)
				{
					y = GridY[x, z];
					GL.DrawElements(BeginMode.Triangles, grayCube.Indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
					GL.Translate(0, y - lastY, 1);
					lastY = y;
				}
				GL.Translate(1, 0, -GridWidth);
			}

			GL.DisableClientState(ArrayCap.VertexArray);
			GL.DisableClientState(ArrayCap.NormalArray);
			GL.DisableClientState(ArrayCap.ColorArray);

			SwapBuffers();
		}
	}
}