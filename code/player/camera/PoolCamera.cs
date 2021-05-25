using Sandbox;
using System;

namespace PoolGame
{
	public partial class PoolCamera : Camera
	{
		public override void Activated()
		{
			if ( Local.Pawn is Player player )
			{
				Pos = player.Position;
				Rot = player.Rotation;
			}

			base.Activated();
		}

		public override void Update()
		{
			if ( Local.Pawn is Player player )
			{
				FieldOfView = 20f;
				Pos = Pos.LerpTo( player.Position, Time.Delta );
				Rot = player.Rotation;
			}

			Viewer = null;
		}
	}
}
