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

			//EnableTouch = true;
		}

		public virtual void OnEnterPocket( TriggerBallPocket pocket )
		{
			var scorer = Game.Instance.CurrentPlayer;

			Log.Info( "Ball Entered Pocket On: " + (Host.IsClient ? "Client" : "Server") );

			if ( IsServer && scorer.IsValid )
			{
				if ( Type == PoolBallType.White )
				{
					Game.Instance.RespawnWhiteBall();
				}
				else
				{
					Game.Instance.RemoveBall( this );
					scorer.Entity.Score++;
				}
			}
		}
	}
}
