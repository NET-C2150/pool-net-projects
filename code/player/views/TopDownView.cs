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
		public float CueYaw { get; set; }

		public override void UpdateCamera( PoolCamera camera )
		{
			var zoomOutDistance = 900f;

			if ( Viewer.Input.Down( InputButton.Attack1 ) )
				zoomOutDistance = 750f;

			camera.Pos = camera.Pos.LerpTo( new Vector3( 0f, 0f, zoomOutDistance ), Time.Delta );
			camera.Rot = Rotation.Lerp( camera.Rot, Rotation.LookAt( Vector3.Down ), Time.Delta );
		}

		public override void Tick()
		{
			if ( !Viewer.IsTurn || Viewer.IsFollowingBall )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var input = Viewer.Input;
			var cue = Viewer.Cue;

			if ( !whiteBall.IsValid || !cue.IsValid )
				return;

			if ( !input.Down( InputButton.Attack1 ) )
			{
				CueYaw += input.MouseDelta.x;
				cue.Entity.WorldRot = Rotation.From( cue.Entity.WorldRot.Angles().WithYaw( CueYaw ) );
			}
			else
			{
				CuePullBackOffset = Math.Clamp( CuePullBackOffset + input.MouseDelta.y, -10f, 60f );

				if ( CuePullBackOffset < -5f )
				{
					using ( Prediction.Off() )
					{
						var force = input.MouseDelta.y * -200f;
						Viewer.StrikeWhiteBall( cue, whiteBall, force );
						return;
					}
				}
			}

			cue.Entity.WorldPos = whiteBall.Entity.WorldPos - cue.Entity.WorldRot.Left * (250f + CuePullBackOffset);

			var tip = cue.Entity.GetAttachment( "tip", true );

			var trace = Trace.Ray( whiteBall.Entity.WorldPos, tip.Pos + cue.Entity.WorldRot.Left * 1000f )
				.Ignore( whiteBall.Entity )
				.Ignore( cue.Entity )
				.Run();

			DebugOverlay.Line( trace.StartPos, trace.EndPos, Color.White );
		}

		public override void OnWhiteBallStruck( PoolCue cue, PoolBall whiteBall, float force )
		{
			CuePullBackOffset = 0f;
		}
	}
}
