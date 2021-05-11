using Sandbox;
using System;

namespace PoolGame
{
	public partial class PoolCamera : BaseCamera
	{
		public override void Activated()
		{
			base.Activated();

			FieldOfView = 70;
		}

		public override void Update()
		{
			if ( Sandbox.Player.Local is not Player player )
				return;

			FieldOfView = 10f;
			Pos = Pos.LerpTo( player.WorldPos, Time.Delta );
			Rot = player.WorldRot;

			Viewer = null;
		}
	}
}
