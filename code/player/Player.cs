using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace PoolGame
{
	public partial class Player : BasePlayer
	{
		[NetLocalPredicted] public BaseView View { get; set; }
		[NetLocalPredicted] public bool IsFollowingBall { get; set; }
		[Net] public PoolBallType BallType { get; set; }

		[Net] public bool IsSpectator { get; private set;  }
		[Net] public bool IsTurn { get; private set; }
		[Net] public int Score { get; set; }
		[Net] public EntityHandle<PoolCue> Cue { get; private set; }

		public Player()
		{
			Controller = new PoolController();
			Camera = new PoolCamera();
			View = new TopDownView()
			{
				Viewer = this
			};
		}

		public void MakeSpectator( bool isSpectator )
		{
			if ( isSpectator )
			{
				ShowPoolCue( false );
			}

			IsFollowingBall = false;
			IsSpectator = isSpectator;
			IsTurn = false;
			Score = 0;
		}

		public void StartPlaying()
		{
			MakeSpectator( false );
			Score = 0;
		}

		public void StartTurn()
		{
			Game.Instance.CurrentPlayer = this;

			IsFollowingBall = false;
			IsTurn = true;

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
			var direction = (whiteBall.WorldPos - cue.WorldPos).Normal;
			var tipPos = cue.GetAttachment( "tip", true ).Pos;

			whiteBall.PhysicsBody.ApplyImpulseAt(
				whiteBall.PhysicsBody.Transform.PointToLocal(tipPos),
				direction * force * whiteBall.PhysicsBody.Mass
			);

			IsFollowingBall = true;

			ShowPoolCue( false );

			View?.OnWhiteBallStruck( cue, whiteBall, force );
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
			View?.Tick();
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}
	}
}
