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
				Position = player.Position;
				Rotation = player.Rotation;
			}

			base.Activated();
		}

		public override void Update()
		{
			if ( Local.Pawn is Player player )
			{
				FieldOfView = 20f;
				Position = Position.LerpTo( player.Position, Time.Delta );
				Rotation = player.Rotation;
			}

			Viewer = null;
		}
	}
}
