using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "pool_ball" )]
	public partial class PoolBall : ModelEntity
	{
		public Player LastStriker { get; private set; }
		public PoolBallType Type { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/pool/pool_ball.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );

			// The World Pool-Billiard Association says so.
			PhysicsBody.Mass = 170f;
			PhysicsBody.LinearDamping = 0.5f;
			PhysicsBody.AngularDamping = 0.5f;

			// Make it harder to lift these balls off the table.
			PhysicsBody.GravityScale = 2f;

			//EnableTouch = true;
		}

		public virtual void OnEnterPocket( TriggerBallPocket pocket )
		{
			Log.Info( "Ball Entered Pocket On: " + (Host.IsClient ? "Client" : "Server") );

			Game.Instance.Round?.OnBallEnterPocket( this, pocket );
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			// Our last striker is the one responsible for this collision.
			if ( eventData.Entity is PoolBall other )
			{
				LastStriker = Game.Instance.CurrentPlayer;

				Game.Instance.Round?.OnBallHitOtherBall( this, other );
			}

			base.OnPhysicsCollision( eventData );
		}
	}
}
