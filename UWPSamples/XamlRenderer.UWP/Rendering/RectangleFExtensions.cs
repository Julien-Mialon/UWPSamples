using SharpDX.Mathematics.Interop;

namespace XamlRenderer.Rendering
{
    public static class RectangleFExtensions
    {
        /// <summary>
        /// Returns a dilated version of the specified rectangle by expanding it by amount in each direction.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static RawRectangleF Dilated(this RawRectangleF rect, float amount)
        {
            rect.Left -= amount;
            rect.Top -= amount;
            rect.Right += amount;
            rect.Bottom += amount;

            return rect;
        }

        /// <summary>
        /// Returns an eroded version of the specified rectangle by shrinking it by amount from each direction.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static RawRectangleF Eroded(this RawRectangleF rect, float amount)
        {
            rect.Left += amount;
            rect.Top += amount;
            rect.Right -= amount;
            rect.Bottom -= amount;

            return rect;
        }

	    public static float Width(this RawRectangleF rect)
	    {
		    return rect.Right - rect.Left;
	    }

		public static float Height(this RawRectangleF rect)
		{
			return rect.Bottom - rect.Top;
		}
	}
}
