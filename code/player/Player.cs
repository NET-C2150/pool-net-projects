using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace PoolGame
{
	public partial class Player : BasePlayer
	{
		public bool IsFollowingWhiteBall { get; set; }

		public PoolCue Cue
		{
			get => (ActiveChild as PoolCue);
		}

		[NetLocalPredicted] public BaseView View { get; set; }

		public Player()
		{
			Controller = new PoolController();
			Camera = new PoolCamera();
			View = new TopDownView()
			{
				Viewer = this
			};
		}

		public void StrikeWhiteBall( PoolCue cue, PoolBall whiteBall, float force )
		{
			var direction = (whiteBall.WorldPos - cue.WorldPos).Normal;
			var tipPos = cue.GetAttachment( "tip", true ).Pos;

			whiteBall.PhysicsBody.ApplyImpulseAt(
				whiteBall.PhysicsBody.Transform.PointToLocal(tipPos),
				direction * 1000f * whiteBall.PhysicsBody.Mass
			);

			ActiveChild.EnableDrawing = false;
			IsFollowingWhiteBall = true;

			View?.OnWhiteBallStriked( cue, whiteBall, force );
		}

		public override void Spawn()
		{
			if ( IsServer )
			{
				var cue = new PoolCue
				{
					Owner = this
				};

				ActiveChild = cue;
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
			TickActiveChild();

			if ( Input.ActiveChild != null )
				ActiveChild = Input.ActiveChild;

			View?.Tick();
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}
	}
}
