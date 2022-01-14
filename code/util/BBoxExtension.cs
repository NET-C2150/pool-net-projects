using System;
using Sandbox;

namespace Facepunch.Pool
{
	public static class BBoxExtension
	{
		public static bool ContainsXY( this BBox a, BBox b )
		{
			return (
				b.Mins.x >= a.Mins.x && b.Maxs.x < a.Maxs.x &&
				b.Mins.y >= a.Mins.y && b.Maxs.y < a.Maxs.y
			); ;
		}

		public static BBox ToWorldSpace( this BBox bbox, ModelEntity entity )
		{
			return new BBox
			{
				Mins = entity.Transform.PointToWorld( bbox.Mins ),
				Maxs = entity.Transform.PointToWorld( bbox.Maxs )
			};
		}
	}
}
