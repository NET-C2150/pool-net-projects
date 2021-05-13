using Sandbox;
using System;

namespace PoolGame
{
	public partial class PoolCamera : BaseCamera
	{
		public override void Update()
		{
			if ( Sandbox.Player.Local is not Player player )
				return;

			FieldOfView = 20f;
			Pos = Pos.LerpTo( player.WorldPos, Time.Delta );
			Rot = player.WorldRot;

			Viewer = null;
		}
	}
}
