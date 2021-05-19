using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace PoolGame
{
	public partial class Player : Entity
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
				var balls = All.OfType<PoolBall>().Where( ( e ) =>
				{
					return e.Type == BallType;
				} );

				return balls.Count();
			}
		}

		public Player()
		{
			Camera = new PoolCamera();
			Transmit = TransmitType.Always;
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
			{
				whiteBall.Entity.StartPlacing();
				whiteBall.Entity.Owner = this;
			}

			_ = Game.Instance.RespawnBallAsync( whiteBall );

			IsPlacingWhiteBall = true;
		}

		public void StopPlacingWhiteBall()
		{
			var whiteBall = Game.Instance.WhiteBall;

			if ( whiteBall.IsValid )
			{
				whiteBall.Entity.StopPlacing();
				whiteBall.Entity.Owner = null;
			}

			IsPlacingWhiteBall = false;
		}

		public void Foul( FoulReason reason )
		{
			if ( FoulReason == FoulReason.None )
			{
				Log.Info( GetClientOwner().Name + " has fouled (reason: " + reason.ToString() + ")" );

				Game.Instance.AddToast( To.Everyone, this, reason.ToMessage( this ), "foul" );

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
			var client = GetClientOwner();

			Log.Info( "Starting Turn: " + client.Name );

			if ( showMessage )
				Game.Instance.AddToast( To.Everyone, this, $"{ client.Name } has started their turn" );

			// This player will be predicting the pool cue now.
			Game.Instance.CurrentPlayer = this;
			Game.Instance.Cue.Entity.Owner = this;

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

		public override void Simulate( Client client )
		{
			var zoomOutDistance = 350f;

			Position = new Vector3( 0f, 0f, zoomOutDistance );
			Rotation = Rotation.LookAt( Vector3.Down );

			base.Simulate( client );
		}
	}
}
