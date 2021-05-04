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
		public WhiteAreaQuad Quad { get; set; }

		public override void Spawn()
		{
			Transmit = TransmitType.Always;

			Quad = new WhiteAreaQuad
			{
				Bounds = CollisionBounds,
				WorldPos = WorldPos,
				Transmit = TransmitType.Always
			};

			base.Spawn();
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );
		}
	}
}
