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
		public ModelEntity GhostBall { get; private set; }

		private float _cuePullBackOffset;
		private float _lastPowerDistance;
		private float _maxCuePitch = 17f;
		private float _minCuePitch = 5f;

		public override void Reset()
		{
			IsMakingShot = false;
		}

		public override void Tick( Player controller )
		{
			var cue = Game.Instance.Cue;

			if ( Host.IsClient )
			{
				if ( ShotPowerLine != null )
					ShotPowerLine.IsEnabled = false;

				if ( GhostBall != null )
					GhostBall.EnableDrawing = false;
			}

			if ( cue.Entity.IsAuthority )
				cue.Entity.EnableDrawing = false;

			if ( !controller.IsTurn || controller.IsFollowingBall )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var client = controller.GetClientOwner();
			var input = client.Input;

			if ( !whiteBall.IsValid || !cue.IsValid )
				return;

			if ( controller.IsPlacingWhiteBall )
			{
				if ( whiteBall.Entity.IsAuthority )
					HandleWhiteBallPlacement( controller, input, whiteBall );

				ShowWhiteArea( true );

				return;
			}
			else
			{
				ShowWhiteArea( false );
			}

			if ( cue.Entity.IsAuthority )
			{
				if ( !input.Down( InputButton.Attack1 ) )
				{
					UpdateAimDir( controller, input, whiteBall.Entity.Position );

					if ( !IsMakingShot )
					{
						RotateCue( input, whiteBall, cue );
					}
					else
					{
						if ( Host.IsServer )
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

			if ( cue.Entity.IsAuthority )
			{
				cue.Entity.EnableDrawing = true;
				cue.Entity.Position = whiteBall.Entity.Position - cue.Entity.Rotation.Forward * (1f + _cuePullBackOffset + (CuePitch * 0.04f));
				cue.Entity.ResetInterpolation();
			}

			if ( Host.IsClient )
			{
				if ( ShotPowerLine == null )
					ShotPowerLine = new ShotPowerLine();

				var trace = Trace.Ray( whiteBall.Entity.Position, whiteBall.Entity.Position + AimDir * 1000f )
					.Ignore( whiteBall.Entity )
					.Ignore( cue.Entity )
					.Run();

				ShotPowerLine.IsEnabled = true;
				ShotPowerLine.Position = trace.StartPos;
				ShotPowerLine.ShotPower = ShotPower;
				ShotPowerLine.EndPos = trace.EndPos;
				ShotPowerLine.Width = 0.1f + ( ( 0.15f / 100f ) * ShotPower );

				var fromTransform = whiteBall.Entity.PhysicsBody.Transform;
				var toTransform = whiteBall.Entity.PhysicsBody.Transform;
				toTransform.Position = trace.EndPos;

				var sweep = Trace.Sweep( whiteBall.Entity.PhysicsBody, fromTransform, toTransform )
					.Ignore( whiteBall )
					.Run();

				if ( sweep.Hit )
				{
					if ( GhostBall == null )
					{
						GhostBall = new ModelEntity();
						GhostBall.SetModel( "models/pool/pool_ball.vmdl" );
						GhostBall.RenderAlpha = 0.4f;
					}

					GhostBall.EnableDrawing = true;
					GhostBall.Position = sweep.EndPos;
				}
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
			var cursorPlaneEndPos = controller.EyePos + input.CursorAim * 350f;
			var distanceToCue = cursorPlaneEndPos.Distance( cue.Entity.Position - cue.Entity.Rotation.Forward * 100f );
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
			if ( IsMakingShot ) return true;

			var tablePlane = new Plane( ballCenter, Vector3.Up );
			var hitPos = tablePlane.Trace( new Ray( controller.EyePos, input.CursorAim ), true );

			if ( !hitPos.HasValue ) return false;

			AimDir = (hitPos.Value - ballCenter).WithZ( 0 ).Normal;

			return true;
		}

		private void RotateCue( UserInput input, EntityHandle<PoolBall> whiteBall, EntityHandle<PoolCue> cue )
		{
			var rollTrace = Trace.Ray( whiteBall.Entity.Position, whiteBall.Entity.Position - AimDir * 100f )
				.Ignore( cue )
				.Ignore( whiteBall )
				.Run();

			var aimRotation = Rotation.LookAt( AimDir, Vector3.Forward );

			_cuePullBackOffset = _cuePullBackOffset.LerpTo( 0f, Time.Delta * 10f );

			CuePitch = CuePitch.LerpTo( _minCuePitch + ((_maxCuePitch - _minCuePitch) * (1f - rollTrace.Fraction)), Time.Delta * 10f );
			CueYaw = aimRotation.Yaw().Normalize( 0f, 360f );

			if ( cue.Entity.IsAuthority )
			{
				cue.Entity.Rotation = Rotation.From(
					cue.Entity.Rotation.Angles()
						.WithYaw( CueYaw )
						.WithPitch( CuePitch )
				);

				cue.Entity.ResetInterpolation();
			}
		}
	}
}
