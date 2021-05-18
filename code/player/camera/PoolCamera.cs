using Sandbox;
using System;

namespace PoolGame
{
	public partial class PoolCamera : Camera
	{
		public override void Update()
		{
			if ( Local.Pawn is not Player player )
				return;

			FieldOfView = 20f;
			Pos = Pos.LerpTo( player.Position, Time.Delta );
			Rot = player.Rotation;

			Viewer = null;
		}
	}
}
