using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public class TopDownView : BaseView
	{
		public float CuePullBackOffset { get; set; }

		public override void UpdateCamera( PoolCamera camera )
		{
			camera.Pos = camera.Pos.LerpTo( new Vector3( 0f, 0f, 1600f ), Time.Delta );
			camera.Rot = Rotation.Lerp( camera.Rot, Rotation.LookAt( Vector3.Down ), Time.Delta );
		}

		public override void Tick()
		{
			if ( Viewer.IsFollowingWhiteBall )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var input = Viewer.Input;
			var cue = Viewer.Cue;

			if ( whiteBall == null || cue == null )
				return;

			if ( !input.Down( InputButton.Attack1 ) )
			{
				var yaw = input.Rot.Yaw();
				cue.WorldRot = Rotation.From( cue.WorldRot.Angles().WithYaw( yaw ) );
			}
			else
			{
				CuePullBackOffset = Math.Clamp( CuePullBackOffset + input.MouseDelta.y, -10f, 60f );

				if ( CuePullBackOffset < -5f )
				{
					using ( Prediction.Off() )
					{
						// TODO: This is shit, it will likely be physics based.
						Log.Info( "Apply Force: " + (Host.IsClient.ToString()) );
						var force = input.MouseDelta.y * 200f;
						Viewer.StrikeWhiteBall( cue, whiteBall, force );
						return;
					}
				}
			}

			cue.EnableDrawing = true;
			cue.WorldPos = whiteBall.WorldPos - cue.WorldRot.Left * (250f + CuePullBackOffset);
		}

		public override void OnWhiteBallStriked( PoolCue cue, PoolBall whiteBall, float force )
		{
			CuePullBackOffset = 0f;
		}
	}
}
