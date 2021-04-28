using Sandbox;

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

			Pos = new Vector3( 0f, 0f, 2048f );
			Rot = Rotation.LookAt( Vector3.Down );

			FieldOfView = FieldOfView.LerpTo( 50, Time.Delta * 3.0f );
			Viewer = null;
		}
	}
}
