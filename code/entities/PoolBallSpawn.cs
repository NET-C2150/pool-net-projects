using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "pool_ball_spawn" )]
	[Hammer.EditorModel( "models/pool/pool_ball.vmdl" )]
	[Hammer.Model]
	public partial class PoolBallSpawn : ModelEntity
	{
		[Property]
		public PoolBallType Type { get; set; }

		[Property]
		public PoolBallNumber Number { get; set; }
	}
}
