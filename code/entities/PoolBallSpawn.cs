using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Pool
{
	[Library( "pool_ball_spawn" )]
	[Hammer.EditorModel( "models/pool/pool_ball.vmdl" )]
	[Display( Name = "Ball Spawnpoint", GroupName = "Pool" )]
	[Hammer.Model]
	public partial class PoolBallSpawn : ModelEntity
	{
		[Property]
		public PoolBallType Type { get; set; }

		[Property]
		public PoolBallNumber Number { get; set; }
	}
}
