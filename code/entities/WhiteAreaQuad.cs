using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "white_area_quad" )]
	public partial class WhiteAreaQuad : RenderEntity
	{
		[Net] public BBox Bounds { get; set; }
		public Material Material = Material.Load( "materials/dev/dev_measuregeneric01.vmat" );

		public override void Spawn()
		{
			base.Spawn();
		}

		public override void DoRender( SceneObject sceneObject  )
		{
			var rect = new Rect( Bounds.Mins.x, Bounds.Mins.y, Bounds.Maxs.x - Bounds.Mins.x, Bounds.Maxs.y - Bounds.Mins.y );

			Render.Color = Color.White;
			Render.Material = Material;
			Render.CullMode = CullMode.None;
			Render.DrawQuad( rect );
		}
	}
}
