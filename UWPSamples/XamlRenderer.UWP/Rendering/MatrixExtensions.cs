using SharpDX.Mathematics.Interop;

namespace XamlRenderer.Rendering
{
	public static class MatrixExtensions
	{
		public static RawMatrix3x2 Identity()
		{
			return new RawMatrix3x2(1,0,0,1,0,0);
		}

		/// <summary>
		/// Creates a matrix that scales along the x-axis and y-axis.
		/// </summary>
		/// <param name="scale">Scaling factor for both axes.</param>
		/// <param name="result">When the method completes, contains the created scaling matrix.</param>
		public static void Scaling(ref RawVector2 scale, out RawMatrix3x2 result)
		{
			Scaling(scale.X, scale.Y, out result);
		}

		/// <summary>
		/// Creates a matrix that scales along the x-axis and y-axis.
		/// </summary>
		/// <param name="scale">Scaling factor for both axes.</param>
		/// <returns>The created scaling matrix.</returns>
		public static RawMatrix3x2 Scaling(RawVector2 scale)
		{
			RawMatrix3x2 result;
			Scaling(ref scale, out result);
			return result;
		}

		/// <summary>
		/// Creates a matrix that scales along the x-axis and y-axis.
		/// </summary>
		/// <param name="x">Scaling factor that is applied along the x-axis.</param>
		/// <param name="y">Scaling factor that is applied along the y-axis.</param>
		/// <param name="result">When the method completes, contains the created scaling matrix.</param>
		public static void Scaling(float x, float y, out RawMatrix3x2 result)
		{
			result = Identity();
			result.M11 = x;
			result.M22 = y;
		}

		/// <summary>
		/// Creates a matrix that scales along the x-axis and y-axis.
		/// </summary>
		/// <param name="x">Scaling factor that is applied along the x-axis.</param>
		/// <param name="y">Scaling factor that is applied along the y-axis.</param>
		/// <returns>The created scaling matrix.</returns>
		public static RawMatrix3x2 Scaling(float x, float y)
		{
			RawMatrix3x2 result;
			Scaling(x, y, out result);
			return result;
		}

		/// <summary>
		/// Creates a matrix that uniformly scales along both axes.
		/// </summary>
		/// <param name="scale">The uniform scale that is applied along both axes.</param>
		/// <param name="result">When the method completes, contains the created scaling matrix.</param>
		public static void Scaling(float scale, out RawMatrix3x2 result)
		{
			result = Identity();
			result.M11 = result.M22 = scale;
		}

		/// <summary>
		/// Creates a matrix that uniformly scales along both axes.
		/// </summary>
		/// <param name="scale">The uniform scale that is applied along both axes.</param>
		/// <returns>The created scaling matrix.</returns>
		public static RawMatrix3x2 Scaling(float scale)
		{
			RawMatrix3x2 result;
			Scaling(scale, out result);
			return result;
		}

		/// <summary>
		/// Creates a matrix that is scaling from a specified center.
		/// </summary>
		/// <param name="x">Scaling factor that is applied along the x-axis.</param>
		/// <param name="y">Scaling factor that is applied along the y-axis.</param>
		/// <param name="center">The center of the scaling.</param>
		/// <returns>The created scaling matrix.</returns>
		public static RawMatrix3x2 Scaling(float x, float y, RawVector2 center)
		{
			RawMatrix3x2 result;

			result.M11 = x; result.M12 = 0.0f;
			result.M21 = 0.0f; result.M22 = y;

			result.M31 = center.X - (x * center.X);
			result.M32 = center.Y - (y * center.Y);

			return result;
		}

		/// <summary>
		/// Creates a matrix that is scaling from a specified center.
		/// </summary>
		/// <param name="x">Scaling factor that is applied along the x-axis.</param>
		/// <param name="y">Scaling factor that is applied along the y-axis.</param>
		/// <param name="center">The center of the scaling.</param>
		/// <param name="result">The created scaling matrix.</param>
		public static void Scaling(float x, float y, ref RawVector2 center, out RawMatrix3x2 result)
		{
			RawMatrix3x2 localResult;

			localResult.M11 = x; localResult.M12 = 0.0f;
			localResult.M21 = 0.0f; localResult.M22 = y;

			localResult.M31 = center.X - (x * center.X);
			localResult.M32 = center.Y - (y * center.Y);

			result = localResult;
		}

	}
}
