using Orbital.Numerics;

namespace Orbital.Video
{
	/// <summary>
	/// Right handed camera
	/// </summary>
	public class Camera
	{
		/// <summary>
		/// Matrix for transforming projected 3D geometry in a view port
		/// </summary>
		public Mat4 matrix;

		/// <summary>
		/// View matrix
		/// </summary>
		public Mat4 viewMatrix;
		
		/// <summary>
		/// Projection matrix
		/// </summary>
		public Mat4 projMatrix;

		/// <summary>
		/// Matrix for transforming billboard objects
		/// </summary>
		public Mat3 billboardMatrix;

		/// <summary>
		/// Target forward vector which is used to generate 'forward' in 'Update'.
		/// This vector does not have to be normalized
		/// </summary>
		public Vec3 targetForward;
		
		/// <summary>
		/// Target up vector which is used to generate 'up' in 'Update'.
		/// This vector does not have to be normalized
		/// </summary>
		public Vec3 targetUp;

		/// <summary>
		/// Position of camera
		/// </summary>
		public Vec3 position;
		
		/// <summary>
		/// Normalized forward vector
		/// </summary>
		public Vec3 forward;
		
		/// <summary>
		/// Normalized up vector
		/// </summary>
		public Vec3 up;
		
		/// <summary>
		/// Normalized right vector
		/// </summary>
		public Vec3 right;

		/// <summary>
		/// Aspect ratio
		/// </summary>
		public float aspect;
		
		/// <summary>
		/// Field of view
		/// </summary>
		public float fov;
		
		/// <summary>
		/// Near clipping plane
		/// </summary>
		public float near;
		
		/// <summary>
		/// Far clipping plane
		/// </summary>
		public float far;

		public Camera()
		{
			targetForward = forward = Vec3.forward;
			targetUp = up = Vec3.up;
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
			forward = targetForward;
			up = targetUp;
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
			targetForward = point - position;
			Update();
		}

		/// <summary>
		/// Updates camera to look at a point in space
		/// </summary>
		public void LookAt(Vec3 forwardPoint, Vec3 upPoint)
		{
			targetForward = forwardPoint - position;
			targetUp = upPoint - position;
			Update();
		}
	}
}
