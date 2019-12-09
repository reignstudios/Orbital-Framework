using Orbital.Numerics;

namespace Orbital.Video
{
	/// <summary>
	/// Right handed camera
	/// </summary>
	public class Camera
	{
		public Mat4 matrix;
		public Mat4 viewMatrix, projMatrix;
		public Mat3 billboardMatrix;
		public Vec3 position, forward, up, right;
		public float aspect, fov, near, far;

		public Camera()
		{
			forward = Vec3.forward;
			up = Vec3.up;
			right = Vec3.right;
			aspect = 1;
			fov = MathTools.DegToRad(45);
			near = 1;
			far = 100;
			Update();
		}

		/// <summary>
		/// Updates camera matrices and vectors from current field values
		/// </summary>
		public void Update()
		{
			viewMatrix = Mat4.ViewRH(position, ref forward, ref up, out right);
			projMatrix = Mat4.Perspective(fov, aspect, near, far);
			matrix = viewMatrix.Multiply(projMatrix);
			billboardMatrix = Mat3.FromCross(-forward, up);
		}

		/// <summary>
		/// Updates aspect, camera matrices and vectors from current field values
		/// </summary>
		public void Update(ViewPort viewPort)
		{
			aspect = viewPort.GetAspect();
			Update();
		}

		/// <summary>
		/// Updates aspect, camera matrices and vectors from current field values
		/// </summary>
		public void Update(float width, float height)
		{
			aspect = width / height;
			Update();
		}

		/// <summary>
		/// Updates camera to look at a point in space
		/// </summary>
		public void LookAt(Vec3 point)
		{
			forward = point - position;
			Update();
		}

		/// <summary>
		/// Updates camera to look at a point in space
		/// </summary>
		public void LookAt(Vec3 forwardPoint, Vec3 upPoint)
		{
			forward = forwardPoint - position;
			up = upPoint - position;
			Update();
		}
	}
}
