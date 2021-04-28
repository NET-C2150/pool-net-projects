using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace PoolGame
{
	public partial class Player : BasePlayer
	{
		public Player()
		{
			Controller = new PoolController();
			Camera = new PoolCamera();
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
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				Log.Info( "Pressed Attack" );

				if ( Host.IsServer )
				{
					var whiteBall = Entity.All.Find( ( e ) => e is PoolBall ball && ball.Type == PoolBallType.White ) as PoolBall;

					if ( whiteBall != null && whiteBall.IsValid() )
					{
						whiteBall.PhysicsBody.Wake();
						Log.Info( "Apply Impulse" );
						whiteBall.ApplyLocalImpulse( new Vector3( 0f, 1000f, 0f ) );
					}
				}
			}

			if ( LifeState != LifeState.Alive )
			{
				return;
			}
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}
	}
}
