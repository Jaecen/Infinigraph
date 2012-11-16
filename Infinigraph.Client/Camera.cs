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
		float TiltAngle = 10;
		float ZoomLevel = 5;
		float PanRate;
		float ZoomRate;
		float RotateRate;

		public Camera(float panRate, float zoomRate, float rotateRate)
		{
			PanRate = panRate;
			ZoomRate = zoomRate;
			RotateRate = rotateRate;
		}

		public void MoveForward(float distance)
		{
			Target.X += (float)(Math.Cos(Rotation) * distance * PanRate);
			Target.Z += (float)(Math.Sin(Rotation) * distance * PanRate);
		}

		public void MoveBackward(float distance)
		{
			Target.X -= (float)(Math.Cos(Rotation) * distance * PanRate);
			Target.Z -= (float)(Math.Sin(Rotation) * distance * PanRate);
		}

		public void MoveLeft(float distance)
		{
			Target.X -= (float)(Math.Sin(-Rotation) * distance * PanRate);
			Target.Z -= (float)(Math.Cos(-Rotation) * distance * PanRate);
		}

		public void MoveRight(float distance)
		{
			Target.X += (float)(Math.Sin(-Rotation) * distance * PanRate);
			Target.Z += (float)(Math.Cos(-Rotation) * distance * PanRate);
		}

		public void Tilt(float amount)
		{
			TiltAngle -= amount * RotateRate;

			if(TiltAngle < 5)
				TiltAngle = 5;
		}

		public void Zoom(float level)
		{
			ZoomLevel -= level * ZoomRate;
			
			if(ZoomLevel < 2)
				ZoomLevel = 2;
		}

		public void Rotate(float amount)
		{
			Rotation -= amount * RotateRate;
		}

		public Vector3 GetTarget()
		{
			return Target;
		}

		public void Apply()
		{
			var location = new Vector3(
				Target.X + (float)(Math.Cos(Rotation + Math.PI) * TiltAngle),
				ZoomLevel,
				Target.Z + (float)(Math.Sin(Rotation + Math.PI) * TiltAngle));

			var lookAt = Matrix4.LookAt(location, Target, CameraUp);
			GL.LoadMatrix(ref lookAt);
		}
	}
}
