using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Pool
{
	[Library( "trigger_ball_pocket" )]
	[Display( Name = "Pocket Trigger", GroupName = "Pool" )]
	public partial class TriggerBallPocket : BaseTrigger
	{
		public override void StartTouch( Entity other )
		{
			if ( other is PoolBall ball )
			{
				ball.OnEnterPocket( this );
			}

			base.StartTouch( other );
		}
	}
}
