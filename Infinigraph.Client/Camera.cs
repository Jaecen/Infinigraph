using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Infinigraph.Client
{
	class Camera
	{
		private readonly Vector3 CameraUp = new Vector3(0, 1, 0);

		Vector3 Target = Vector3.Zero;
		float Rotation = 0;
		float TiltLevel = 10;
		float ZoomLevel = 5;

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

		public void Tilt(float level)
		{
			TiltLevel -= level;

			if(TiltLevel < 5)
				TiltLevel = 5;
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
				Target.X + (float)(Math.Cos(Rotation + Math.PI) * TiltLevel),
				ZoomLevel,
				Target.Z + (float)(Math.Sin(Rotation + Math.PI) * TiltLevel));

			var lookAt = Matrix4.LookAt(location, Target, CameraUp);
			GL.LoadMatrix(ref lookAt);
		}
	}
}
