using Sandbox;
using Sandbox.UI;
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
		public float CueRoll { get; set; }
		public float CueYaw { get; set; }

		private float _maxCueRoll = 35f;
		private float _minCueRoll = 5f;

		public override void UpdateCamera( PoolCamera camera )
		{
			camera.Pos = camera.Pos.LerpTo( Viewer.WorldPos, Time.Delta );
			camera.Rot = Rotation.Lerp( camera.Rot, Viewer.WorldRot, Time.Delta );
		}

		public override void Tick()
		{
			var zoomOutDistance = 900f;

			if ( Viewer.Input.Down( InputButton.Attack1 ) )
				zoomOutDistance = 750f;

			Viewer.WorldPos = new Vector3( 0f, 0f, zoomOutDistance );
			Viewer.WorldRot = Rotation.LookAt( Vector3.Down );

			if ( !Viewer.IsTurn || Viewer.IsFollowingBall )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var input = Viewer.Input;
			var cue = Viewer.Cue;

			if ( !whiteBall.IsValid || !cue.IsValid )
				return;

			if ( Host.IsServer && Viewer.IsPlacingWhiteBall )
			{
				var moveVector = new Vector3( -input.MouseDelta.y, -input.MouseDelta.x ) * Time.Delta * 20f;
				whiteBall.Entity.WorldPos += moveVector;
				return;
			}

			if ( !input.Down( InputButton.Attack1 ) )
			{
				var direction = cue.Entity.DirectionTo( whiteBall );
				var rollTrace = Trace.Ray( whiteBall.Entity.WorldPos, whiteBall.Entity.WorldPos - direction * 100f )
					.Ignore( cue )
					.Ignore( whiteBall )
					.Run();

				CuePullBackOffset = CuePullBackOffset.LerpTo( 0f, Time.Delta * 10f );
				CueYaw = (CueYaw + (input.MouseDelta.x * Time.Delta * 20f)).Normalize( 0f, 360f );
				CueRoll = CueRoll.LerpTo( _minCueRoll + ((_maxCueRoll - _minCueRoll) * (1f - rollTrace.Fraction)), Time.Delta * 10f );

				Log.Info( CueYaw.ToString() );

				cue.Entity.WorldRot = Rotation.From(
					cue.Entity.WorldRot.Angles()
						.WithYaw( CueYaw )
						.WithRoll( -CueRoll )
				);
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

			cue.Entity.WorldPos = whiteBall.Entity.WorldPos - cue.Entity.WorldRot.Left * (250f + CuePullBackOffset + (CueRoll * 0.3f));

			var trace = Trace.Ray( whiteBall.Entity.WorldPos, whiteBall.Entity.WorldPos + cue.Entity.DirectionTo( whiteBall.Entity) * 1000f )
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
