using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace PoolGame
{
	public partial class Player : BasePlayer
	{
		[Net] public bool IsFollowingBall { get; set; }
		[Net] public PoolBallType BallType { get; set; }
		[Net] public bool IsSpectator { get; private set;  }
		[Net] public FoulReason FoulReason { get; private set; }
		[Net] public bool IsPlacingWhiteBall { get; private set; }
		[Net] public bool HasSecondShot { get; set; }
		[Net] public bool IsTurn { get; private set; }
		[Net] public int Score { get; set; }
		[Net] public EloScore Elo { get; private set; }
		public bool DidHitOwnBall { get; set; }
		public bool DidPotBall { get; set; }

		public int BallsLeft
		{
			get
			{
				var balls = Entity.All.OfType<PoolBall>().Where( ( e ) =>
				{
					return e.Type == BallType;
				} );

				return balls.Count();
			}
		}

		public Player()
		{
			Controller = new PoolPlayerController();
			Camera = new PoolCamera();
		}

		public void MakeSpectator( bool isSpectator )
		{
			IsFollowingBall = false;
			IsSpectator = isSpectator;
			IsTurn = false;
			Score = 0;
		}

		public void StartPlacingWhiteBall()
		{
			var whiteBall = Game.Instance.WhiteBall;

			if ( whiteBall.IsValid )
				whiteBall.Entity.StartPlacing();

			_ = Game.Instance.RespawnBallAsync( whiteBall );

			IsPlacingWhiteBall = true;
		}

		public void StopPlacingWhiteBall()
		{
			var whiteBall = Game.Instance.WhiteBall;

			if ( whiteBall.IsValid )
				whiteBall.Entity.StopPlacing();

			IsPlacingWhiteBall = false;
		}

		public void Foul( FoulReason reason )
		{
			if ( FoulReason == FoulReason.None )
			{
				Log.Info( Name + " has fouled (reason: " + reason.ToString() + ")" );

				Game.Instance.AddToast( this, reason.ToMessage( this ), "foul" );

				HasSecondShot = false;
				FoulReason = reason;
			}
		}

		public void StartPlaying()
		{
			MakeSpectator( false );
			BallType = PoolBallType.White;
			Score = 0;
		}

		public void StartTurn(bool hasSecondShot = false, bool showMessage = true)
		{
			Log.Info( "Starting Turn: " + Name );

			if ( showMessage )
				Game.Instance.AddToast( this, $"{Name} has started their turn" );

			Game.Instance.CurrentPlayer = this;

			IsFollowingBall = false;
			HasSecondShot = hasSecondShot;
			FoulReason = FoulReason.None;
			DidHitOwnBall = false;
			DidPotBall = false;
			IsTurn = true;

			if ( hasSecondShot )
				StartPlacingWhiteBall();
		}

		public void FinishTurn()
		{
			IsFollowingBall = false;
			IsTurn = false;
		}

		public void StrikeWhiteBall( PoolCue cue, PoolBall whiteBall, float force )
		{
			var direction = cue.DirectionTo( whiteBall );

			whiteBall.PhysicsBody.ApplyImpulse( direction * force * whiteBall.PhysicsBody.Mass );

			IsFollowingBall = true;
		}

		public override void Respawn()
		{
			Game.Instance?.Round?.OnPlayerSpawn( this );

			base.Respawn();
		}

		protected override void Tick()
		{
			var zoomOutDistance = 350f;

			WorldPos = new Vector3( 0f, 0f, zoomOutDistance );
			WorldRot = Rotation.LookAt( Vector3.Down );

			var currentPlayer = Game.Instance.CurrentPlayer;

			if ( currentPlayer == this )
				Game.Instance.Controller.Tick( this );

			base.Tick();
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}
	}
}
