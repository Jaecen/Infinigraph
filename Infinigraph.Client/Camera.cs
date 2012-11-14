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

		Vector3 Target = Vector3.Zero;
		float Rotation = 0;
		float ZoomLevel = 15;

		public void MoveForward(float distance)
		{
			Target.X += (float)(Math.Cos(Rotation) * distance);
			Target.Z += (float)(Math.Sin(Rotation) * distance);
		}

		public void MoveBackward(float distance)
		{
			Target.X -= (float)(Math.Cos(Rotation) * distance);
			Target.Z -= (float)(Math.Sin(Rotation) * distance);
		}

		public void MoveLeft(float distance)
		{
			Target.X -= (float)(Math.Sin(-Rotation) * distance);
			Target.Z -= (float)(Math.Cos(-Rotation) * distance);
		}

		public void MoveRight(float distance)
		{
			Target.X += (float)(Math.Sin(-Rotation) * distance);
			Target.Z += (float)(Math.Cos(-Rotation) * distance);
		}

		public void Zoom(float level)
		{
			ZoomLevel -= level;
			
			if(ZoomLevel < 2)
				ZoomLevel = 2;
		}

		public void Rotate(float amount)
		{
			Rotation -= amount;
		}

		public void Apply()
		{
			var location = new Vector3(
				Target.X + (float)(Math.Cos(Rotation + Math.PI) * 5),
				ZoomLevel,
				Target.Z + (float)(Math.Sin(Rotation + Math.PI) * 5));

			var lookAt = Matrix4.LookAt(location, Target, CameraUp);
			GL.LoadMatrix(ref lookAt);
		}
	}
}
