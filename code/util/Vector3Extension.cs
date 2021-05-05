using System;

namespace Sandbox.UI
{
	public static class Vector3Extension
	{
		public static Vector3 TemporaryNormalFix( this Vector3 value )
		{
			// TODO: This is a temporary fix, sometimes Normal returns negative zero values.

			var distance = MathF.Sqrt( value.x * value.x + value.y * value.y + value.z * value.z );
			var output = new Vector3( value.x / distance, value.y / distance, value.z / distance );

			if ( output.x >= -0.01f && output.x <= 0.01f )
				output.x = 0f;

			if ( output.y >= -0.01f && output.y <= 0.01f )
				output.y = 0f;

			if ( output.z >= -0.01f && output.z <= 0.01f )
				output.z = 0f;

			return output;
		}
	}
}
