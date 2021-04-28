using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "pool_cue" )]
	public partial class PoolCue : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/pool/pool_cue.vmdl" );

			EnableDrawing = false;
		}
	}
}
