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
		[Net] public bool DidPotBall { get; set; }
		[Net] public bool IsTurn { get; private set; }
		[Net] public int Score { get; set; }
		[Net] public EntityHandle<PoolCue> Cue { get; private set; }
		[Net] public EloScore Elo { get; private set; }

		public int BallsLeft
		{
			get
			{
				var balls = Entity.All.Where( ( e ) =>
				{
					return (e is PoolBall ball && ball.Type == BallType);
				} );

				return balls.Count();
			}
		}

		public Player()
		{
			Controller = new PoolController();
			Camera = new PoolCamera();
		}

		public void MakeSpectator( bool isSpectator )
		{
			if ( isSpectator )
				ShowPoolCue( false );

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

			ShowPoolCue( false );
		}

		public void StopPlacingWhiteBall()
		{
			var whiteBall = Game.Instance.WhiteBall;

			if ( whiteBall.IsValid )
				whiteBall.Entity.StopPlacing();

			IsPlacingWhiteBall = false;

			if ( IsTurn )
				ShowPoolCue( true );
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

		public void StartTurn(bool hasSecondShot = false)
		{
			Log.Info( "Starting Turn: " + Name );

			Game.Instance.AddToast( this, $"{Name} has started their turn" );
			Game.Instance.CurrentPlayer = this;

			IsFollowingBall = false;
			HasSecondShot = hasSecondShot;
			FoulReason = FoulReason.None;
			DidPotBall = false;
			IsTurn = true;

			if ( hasSecondShot )
				StartPlacingWhiteBall();
			else
				ShowPoolCue( true );
		}

		public void FinishTurn()
		{
			IsFollowingBall = false;
			ShowPoolCue( false );
			IsTurn = false;
		}

		public void ShowPoolCue( bool shouldShow )
		{
			Cue.Entity.EnableDrawing = shouldShow;
		}

		public void StrikeWhiteBall( PoolCue cue, PoolBall whiteBall, float force )
		{
			var direction = cue.DirectionTo( whiteBall );
			var tipPos = cue.GetAttachment( "tip", true ).Pos;

			whiteBall.PhysicsBody.ApplyImpulseAt(
				whiteBall.PhysicsBody.Transform.PointToLocal(tipPos),
				direction * force * whiteBall.PhysicsBody.Mass
			);

			IsFollowingBall = true;

			ShowPoolCue( false );
		}

		public override void Spawn()
		{
			if ( IsServer )
			{
				Cue = new PoolCue
				{
					Owner = this
				};
			}

			base.Spawn();
		}

		public override void Respawn()
		{
			Game.Instance?.Round?.OnPlayerSpawn( this );

			base.Respawn();
		}

		protected override void Tick()
		{
			var zoomOutDistance = 700f;

			WorldPos = new Vector3( 0f, 0f, zoomOutDistance );
			WorldRot = Rotation.LookAt( Vector3.Down );

			base.Tick();
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}
	}
}
