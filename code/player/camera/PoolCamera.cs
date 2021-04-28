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

			player.View?.UpdateCamera( this );

			Viewer = null;
		}

		public override void BuildInput( ClientInput input )
		{
			if ( Sandbox.Player.Local is Player player )
				player.View?.BuildInput( input );

			base.BuildInput( input );
		}
	}
}
