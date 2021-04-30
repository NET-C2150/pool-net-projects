using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "trigger_white_area" )]
	public partial class TriggerWhiteArea : BaseTrigger
	{
		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );
		}
	}
}
