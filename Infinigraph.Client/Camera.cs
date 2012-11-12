using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinigraph.Client
{
	class Camera
	{
		private readonly Vector3 CameraUp = new Vector3(0, 1, 0);

		Matrix4 Delta = Matrix4.Identity;
		float Rotation = 0;

		public void MoveForward(float distance)
		{
			Delta = Matrix4.Mult(Delta, Matrix4.CreateTranslation(0, 0, distance));
		}

		public void MoveBackward(float distance)
		{
			Delta = Matrix4.Mult(Delta, Matrix4.CreateTranslation(0, 0, -distance));
		}

		public void MoveLeft(float distance)
		{
			Delta = Matrix4.Mult(Delta, Matrix4.CreateTranslation(distance, 0, 0));
		}

		public void MoveRight(float distance)
		{
			Delta = Matrix4.Mult(Delta, Matrix4.CreateTranslation(-distance, 0, 0));
		}

		public void Zoom(float level)
		{
			//Location.Y -= level;

			//if(Location.Y < 1)
			//	Location.Y = 1;
		}

		public void Rotate(float amount)
		{
			Rotation += amount;
		}

		public void Apply()
		{
			var target = Vector3.Transform(new Vector3(0, 0, 0), Delta);
			var locationTrans = Matrix4.CreateTranslation(10 * (float)Math.Cos(Rotation), 0, 10 * (float)Math.Sin(Rotation));
			var location = new Vector3(target.X - 5, 15, target.Y - 5);

			var lookAt = Matrix4.LookAt(location, target, CameraUp);



			GL.LoadMatrix(ref lookAt);
			
			
			//GL.Translate(location.X, 0, location.Z);
			//GL.Rotate(Rotation, CameraUp);
			//GL.Translate(-location.X, 0, -location.Z);
		}
	}
}
