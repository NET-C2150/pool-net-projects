using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "pool_ball_spawn" )]
	public partial class PoolBallSpawn : ModelEntity
	{
		[HammerProp]
		public int Type { get; set; }
	}
}
