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

			//EnableTouch = true;
		}

		public virtual void OnEnterPocket( TriggerBallPocket pocket )
		{

		}
	}
}
