using System;
using Sandbox;

namespace PoolGame
{
	public static class BBoxExtension
	{
		public static bool ContainsXY( this BBox a, BBox b )
		{
			return (
				b.Mins.X >= a.Mins.X && b.Maxs.X < a.Maxs.X &&
				b.Mins.Y >= a.Mins.Y && b.Maxs.Y < a.Maxs.Y
			); ;
		}

		public static bool Contains( this BBox a, BBox b )
		{
			return (
				b.Mins.X >= a.Mins.X && b.Maxs.X < a.Maxs.X &&
				b.Mins.Y >= a.Mins.Y && b.Maxs.Y < a.Maxs.Y &&
				b.Mins.Z >= a.Mins.Z && b.Maxs.Z < a.Maxs.Z
			); ;
		}

		public static bool Overlaps( this BBox a, BBox b )
		{
			return (
				a.Mins.X < b.Maxs.X && b.Mins.X < a.Maxs.X &&
				a.Mins.Y < b.Maxs.Y && b.Mins.Y < a.Maxs.Y &&
				a.Mins.Z < b.Maxs.Z && b.Mins.Z < a.Maxs.Z
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
