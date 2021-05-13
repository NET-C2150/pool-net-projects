using Sandbox;
using Sandbox.UI;
using System;

namespace PoolGame
{
	public partial class TopDownController : BaseGameController
	{
		[Net] public Vector3 AimDir { get; set; }
		[Net] public float ShotPower { get; set; }
		public bool IsMakingShot { get; set; }
		public float CuePitch { get; set; }
		public float CueYaw { get; set; }
		public ShotPowerLine ShotPowerLine { get; set; }

		private float _cuePullBackOffset;
		private float _lastPowerDistance;
		private float _maxCuePitch = 35f;
		private float _minCuePitch = 5f;

		public override void Reset()
		{
			IsMakingShot = false;
		}

		public override void Tick( Player controller )
		{
			if ( Host.IsClient && ShotPowerLine != null )
				ShotPowerLine.IsEnabled = false;

			if ( !controller.IsTurn || controller.IsFollowingBall )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var input = controller.Input;
			var cue = controller.Cue;

			if ( !whiteBall.IsValid || !cue.IsValid )
				return;

			if ( controller.IsPlacingWhiteBall )
			{
				if ( Host.IsServer )
					HandleWhiteBallPlacement( controller, input, whiteBall );

				ShowWhiteArea( true );

				return;
			}
			else
			{
				ShowWhiteArea( false );
			}

			if ( Host.IsServer )
			{
				if ( !input.Down( InputButton.Attack1 ) )
				{
					UpdateAimDir( controller, input, whiteBall.Entity.WorldPos );

					if ( !IsMakingShot )
					{
						RotateCueToCursor( input, whiteBall, cue );
					}
					else
					{
						TakeShot( controller, cue, whiteBall );

						_cuePullBackOffset = 0f;
						IsMakingShot = false;
						ShotPower = 0f;
					}
				}
				else
				{
					HandlePowerSelection( controller, input, cue );
				}
			}

			if ( Host.IsServer )
				cue.Entity.WorldPos = whiteBall.Entity.WorldPos - cue.Entity.WorldRot.Forward * (1f + _cuePullBackOffset + (CuePitch * 0.04f));

			if ( Host.IsClient )
			{
				if ( ShotPowerLine == null )
					ShotPowerLine = new ShotPowerLine();

				var trace = Trace.Ray( whiteBall.Entity.WorldPos, whiteBall.Entity.WorldPos + AimDir * 1000f )
					.Ignore( whiteBall.Entity )
					.Ignore( cue.Entity )
					.Run();

				ShotPowerLine.IsEnabled = true;
				ShotPowerLine.WorldPos = trace.StartPos;
				ShotPowerLine.ShotPower = ShotPower;
				ShotPowerLine.EndPos = trace.EndPos;
				ShotPowerLine.Width = 0.1f + ( ( 0.15f / 100f ) * ShotPower );
			}
		}

		private void ShowWhiteArea( bool shouldShow )
		{
			if ( Host.IsServer ) return;

			var whiteArea = Game.Instance.WhiteArea;

			if ( whiteArea != null && whiteArea.IsValid() )
				whiteArea.Quad.IsEnabled = shouldShow;
		}

		private void TakeShot( Player controller, EntityHandle<PoolCue> cue, EntityHandle<PoolBall> whiteBall )
		{
			Host.AssertServer();

			if ( ShotPower >= 5f )
			{
				using ( Prediction.Off() )
				{
					controller.StrikeWhiteBall( cue, whiteBall, ShotPower * 6f );

					var soundFileId = Convert.ToInt32( MathF.Round( (2f / 100f) * ShotPower ) );
					whiteBall.Entity.PlaySound( $"shot-power-{soundFileId}" );
				}
			}
		}

		private void HandleWhiteBallPlacement( Player controller, UserInput input, EntityHandle<PoolBall> whiteBall )
		{
			Host.AssertServer();

			var cursorTrace = Trace.Ray( controller.EyePos, controller.EyePos + input.CursorAim * 1000f )
				.WorldOnly()
				.Run();

			var whiteArea = Game.Instance.WhiteArea;
			var whiteAreaWorldOBB = whiteArea.CollisionBounds.ToWorldSpace( whiteArea );

			whiteBall.Entity.TryMoveTo( cursorTrace.EndPos, whiteAreaWorldOBB );

			if ( input.Released( InputButton.Attack1 ) )
				controller.StopPlacingWhiteBall();
		}

		private void HandlePowerSelection( Player controller, UserInput input, EntityHandle<PoolCue> cue )
		{
			Host.AssertServer();

			var cursorPlaneEndPos = controller.EyePos + input.CursorAim * 700f;
			var distanceToCue = cursorPlaneEndPos.Distance( cue.Entity.WorldPos - cue.Entity.WorldRot.Forward * 100f );
			var cuePullBackDelta = (_lastPowerDistance - distanceToCue) * Time.Delta * 20f;

			if ( !IsMakingShot )
			{
				_lastPowerDistance = 0f;
				cuePullBackDelta = 0f;
			}

			_cuePullBackOffset = Math.Clamp( _cuePullBackOffset + cuePullBackDelta, 0f, 8f );
			_lastPowerDistance = distanceToCue;

			IsMakingShot = true;
			ShotPower = _cuePullBackOffset.AsPercentMinMax( 0f, 8f );
		}

		private bool UpdateAimDir( Player controller, UserInput input, Vector3 ballCenter )
		{
			Host.AssertServer();

			if ( IsMakingShot ) return true;

			var tablePlane = new Plane( ballCenter, Vector3.Up );
			var hitPos = tablePlane.Trace( new Ray( controller.EyePos, input.CursorAim ), true );

			if ( !hitPos.HasValue ) return false;

			AimDir = (hitPos.Value - ballCenter).WithZ( 0 ).Normal;

			return true;
		}

		private void RotateCueToCursor( UserInput input, EntityHandle<PoolBall> whiteBall, EntityHandle<PoolCue> cue )
		{
			Host.AssertServer();

			var rollTrace = Trace.Ray( whiteBall.Entity.WorldPos, whiteBall.Entity.WorldPos - AimDir * 100f )
				.Ignore( cue )
				.Ignore( whiteBall )
				.Run();

			var aimRotation = Rotation.LookAt( AimDir, Vector3.Forward );

			_cuePullBackOffset = _cuePullBackOffset.LerpTo( 0f, Time.Delta * 10f );

			CuePitch = CuePitch.LerpTo( _minCuePitch + ((_maxCuePitch - _minCuePitch) * (1f - rollTrace.Fraction)), Time.Delta * 10f );
			CueYaw = aimRotation.Yaw().Normalize( 0f, 360f );

			cue.Entity.WorldRot = Rotation.From(
				cue.Entity.WorldRot.Angles()
					.WithYaw( CueYaw )
					.WithPitch( CuePitch )
			);
		}
	}
}
