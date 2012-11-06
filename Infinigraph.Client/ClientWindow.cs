using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinigraph.Client
{

	class ClientWindow : GameWindow
	{
		string vertexShaderSource = @"
			void main()
			{
				gl_FrontColor = gl_Color;
				gl_Position = ftransform();
			}";

		string fragmentShaderSource = @"
			void main()
			{
				gl_FragColor = gl_Color;
			}";

        #region --- Fields ---

        static float angle = 0.0f, rotation_speed = 3.0f;
        int vertex_shader_object, fragment_shader_object, shader_program;
        int vertex_buffer_object, color_buffer_object, element_buffer_object;

        Examples.Shapes.Shape shape = new Examples.Shapes.Cube();

        #endregion

        #region --- Constructors ---

		public ClientWindow()
            : base(800, 600, GraphicsMode.Default)
        { }

        #endregion

        #region OnLoad

        /// <summary>
        /// This is the place to load resources that change little
        /// during the lifetime of the GameWindow. In this case, we
        /// check for GLSL support, and load the shaders.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Check for necessary capabilities:
            string version = GL.GetString(StringName.Version);
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major < 2)
            {
                Console.WriteLine("You need at least OpenGL 2.0 to run this example. Aborting.");
                this.Exit();
            }

            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.DepthTest);

            CreateVBO();

			CreateShaders(vertexShaderSource, fragmentShaderSource,
                out vertex_shader_object, out fragment_shader_object,
                out shader_program);
        }

        #endregion

        #region CreateShaders

        void CreateShaders(string vs, string fs,
            out int vertexObject, out int fragmentObject, 
            out int program)
        {
            int status_code;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            // Compile vertex shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out status_code);
            
            if (status_code != 1)
                throw new ApplicationException(info);

            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);

            GL.LinkProgram(program);
            GL.UseProgram(program);
        }

        #endregion

        #region private void CreateVBO()

        void CreateVBO()
        {
            int size;

            GL.GenBuffers(1, out vertex_buffer_object);
            GL.GenBuffers(1, out color_buffer_object);
            GL.GenBuffers(1, out element_buffer_object);

            // Upload the vertex buffer.
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertex_buffer_object);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Vertices.Length * 3 * sizeof(float)), shape.Vertices,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (size != shape.Vertices.Length * 3 * sizeof(Single))
                throw new ApplicationException(String.Format(
                    "Problem uploading vertex buffer to VBO (vertices). Tried to upload {0} bytes, uploaded {1}.",
                    shape.Vertices.Length * 3 * sizeof(Single), size));

            // Upload the color buffer.
            GL.BindBuffer(BufferTarget.ArrayBuffer, color_buffer_object);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(shape.Colors.Length * sizeof(int)), shape.Colors,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (size != shape.Colors.Length * sizeof(int))
                throw new ApplicationException(String.Format(
                    "Problem uploading vertex buffer to VBO (colors). Tried to upload {0} bytes, uploaded {1}.",
                    shape.Colors.Length * sizeof(int), size));
            
            // Upload the index buffer (elements inside the vertex buffer, not color indices as per the IndexPointer function!)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, element_buffer_object);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(shape.Indices.Length * sizeof(Int32)), shape.Indices,
                BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (size != shape.Indices.Length * sizeof(int))
                throw new ApplicationException(String.Format(
                    "Problem uploading vertex buffer to VBO (offsets). Tried to upload {0} bytes, uploaded {1}.",
                    shape.Indices.Length * sizeof(int), size));
        }

        #endregion

        #region OnUnload

        protected override void OnUnload(EventArgs e)
        {
            if (shader_program != 0)
                GL.DeleteProgram(shader_program);
            if (fragment_shader_object != 0)
                GL.DeleteShader(fragment_shader_object);
            if (vertex_shader_object != 0)
                GL.DeleteShader(vertex_shader_object);
            if (vertex_buffer_object != 0)
                GL.DeleteBuffers(1, ref vertex_buffer_object);
            if (element_buffer_object != 0)
                GL.DeleteBuffers(1, ref element_buffer_object);
        }

        #endregion

        #region OnResize

        /// <summary>
        /// Called when the user resizes the window.
        /// </summary>
        /// <param name="e">Contains the new width/height of the window.</param>
        /// <remarks>
        /// You want the OpenGL viewport to match the window. This is the place to do it!
        /// </remarks>
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
        }

        #endregion

        #region OnUpdateFrame

        /// <summary>
        /// Prepares the next frame for rendering.
        /// </summary>
        /// <remarks>
        /// Place your control logic here. This is the place to respond to user input,
        /// update object positions etc.
        /// </remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();

            if (Keyboard[OpenTK.Input.Key.F11])
                if (WindowState != WindowState.Fullscreen)
                    WindowState = WindowState.Fullscreen;
                else
                    WindowState = WindowState.Normal;
        }

        #endregion

        #region OnRenderFrame

        /// <summary>
        /// Place your rendering code here.
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit);

            Matrix4 lookat = Matrix4.LookAt(0, 5, 5, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            angle += rotation_speed * (float)e.Time;
            GL.Rotate(angle, 0.0f, 1.0f, 0.0f);

            GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.ColorArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertex_buffer_object);
            GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ArrayBuffer, color_buffer_object);
            GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, IntPtr.Zero);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, element_buffer_object);

            GL.DrawElements(BeginMode.Triangles, shape.Indices.Length,
                DrawElementsType.UnsignedInt, IntPtr.Zero);

            //GL.DrawArrays(GL.Enums.BeginMode.POINTS, 0, shape.Vertices.Length);

			GL.DisableClientState(ArrayCap.VertexArray);
			GL.DisableClientState(ArrayCap.ColorArray);
            

            //int error = GL.GetError();
            //if (error != 0)
            //    Debug.Print(Glu.ErrorString(Glu.Enums.ErrorCode.INVALID_OPERATION));

            SwapBuffers();
        }

        #endregion
	}
}
