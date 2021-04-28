using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public abstract class BaseView : NetworkClass
	{
		public Player Viewer { get; set; }

		public BaseView()
		{
			if ( Host.IsClient && Sandbox.Player.Local is Player player )
				Viewer = player;
		}

		public virtual void UpdateCamera( PoolCamera camera ) { }

		public virtual void Tick() { }

		public virtual void BuildInput( ClientInput input ) { }

		public virtual void OnWhiteBallStriked( PoolCue cue, PoolBall whiteBall, float force ) { }
	}
}
