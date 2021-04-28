using Sandbox;
using System;

namespace PoolGame
{
	public partial class PoolCamera : BaseCamera
	{
		public float HeightOffset { get; set; }

		public override void Activated()
		{
			base.Activated();

			FieldOfView = 70;
		}

		public override void Update()
		{
			if ( Sandbox.Player.Local is not Player player )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var cue = player.ActiveChild as PoolCue;

			if ( whiteBall != null && cue != null )
			{
				Pos = cue.WorldPos + new Vector3( 0f, 0f, 50f + HeightOffset );
				var difference = (whiteBall.WorldPos - Pos).Normal;
				Rot = Rotation.LookAt( difference, Vector3.Up );
			}
			else
            {
				Pos = Pos.LerpTo( new Vector3( 0f, 0f, 2048f ), Time.Delta );
				Rot = Rotation.Lerp( Rot, Rotation.LookAt( Vector3.Down ), Time.Delta );
			}

			FieldOfView = FieldOfView.LerpTo( 50, Time.Delta * 3.0f );
			Viewer = null;
		}

		public override void BuildInput( ClientInput input )
		{
			if ( !input.Down( InputButton.Attack1 ) )
			{
				HeightOffset = Math.Clamp( HeightOffset + input.AnalogLook.pitch, 0f, 200f );
			}

			base.BuildInput( input );
		}
	}
}
