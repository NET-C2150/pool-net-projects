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
