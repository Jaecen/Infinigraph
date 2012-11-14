﻿using OpenTK;
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

		const float cameraPanRate = 10f;
		const float cameraZoomRate = 20f;
		const float cameraRotateRate = (float)(Math.PI / 2);
		Camera Camera = new Camera();

		const int GridWidth = 50;
		const int GridHeight = 50;
		Grid Grid = new Grid(
			GridWidth,
			GridHeight, 
			(x, z) => Noise.Generate(x / (float)GridWidth * 5, z / (float)GridHeight * 5) * 2);

		public ClientWindow()
			: base(800, 600)
		{ }

		// This is the place to load resources that change little during the lifetime of the GameWindow. 
		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(Color.MidnightBlue);
			GL.Enable(EnableCap.DepthTest);

			VertexShader = CreateShader(VertexShaderSource, ShaderType.VertexShader);
			FragmentShader = CreateShader(FragmentShaderSource, ShaderType.FragmentShader);
			ShaderProgram = CreateShaderProgram(VertexShader, FragmentShader);

			Grid.LoadBuffers();
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

		protected override void OnUnload(EventArgs e)
		{
			if(ShaderProgram != 0)
				GL.DeleteProgram(ShaderProgram);

			if(FragmentShader != 0)
				GL.DeleteShader(FragmentShader);

			if(VertexShader != 0)
				GL.DeleteShader(VertexShader);

			if(Grid != null)
				Grid.DeleteBuffers();
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

			if(Keyboard[OpenTK.Input.Key.Left] || Keyboard[OpenTK.Input.Key.A])
				Camera.MoveLeft((float)(e.Time * cameraPanRate));

			if(Keyboard[OpenTK.Input.Key.Right] || Keyboard[OpenTK.Input.Key.D])
				Camera.MoveRight((float)(e.Time * cameraPanRate));

			if(Keyboard[OpenTK.Input.Key.Up] || Keyboard[OpenTK.Input.Key.W])
				Camera.MoveForward((float)(e.Time * cameraPanRate));

			if(Keyboard[OpenTK.Input.Key.Down] || Keyboard[OpenTK.Input.Key.S])
				Camera.MoveBackward((float)(e.Time * cameraPanRate));

			if(Keyboard[OpenTK.Input.Key.R])
				Camera.Zoom((float)(e.Time * cameraZoomRate));

			if(Keyboard[OpenTK.Input.Key.F])
				Camera.Zoom((float)(e.Time * -cameraZoomRate));

			if(Keyboard[OpenTK.Input.Key.Q])
				Camera.Rotate((float)(e.Time * cameraRotateRate));

			if(Keyboard[OpenTK.Input.Key.E])
				Camera.Rotate((float)(e.Time * -cameraRotateRate));
		}

		// Place your rendering code here.
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.MatrixMode(MatrixMode.Modelview);

			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.NormalArray);
			GL.EnableClientState(ArrayCap.ColorArray);

			// Draw grid
			Grid.BindBuffers();
			Grid.DrawGrid();

			Camera.Apply();

			GL.DisableClientState(ArrayCap.VertexArray);
			GL.DisableClientState(ArrayCap.NormalArray);
			GL.DisableClientState(ArrayCap.ColorArray);

			SwapBuffers();
		}
	}
}