using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "shot_power_arrow" )]
	public partial class ShotPowerLine : RenderEntity
	{
		public Material PowerCircleMaterial = Material.Load( "materials/pool_power_circle.vmat" );
		public Material LineMaterial = Material.Load( "materials/pool_cue_line.vmat" );
		public bool IsEnabled { get; set; }
		public float ShotPower { get; set; }
		public Color Color { get; set; }
		public Vector3 EndPos { get; set; }
		public float Width { get; set; } = 1f;

		public override void DoRender( SceneObject sceneObject  )
		{
			if ( IsEnabled  )
			{
				Render.SetLighting( sceneObject );

				var vertexBuffer = Render.GetDynamicVB( true );
				var widthOffset = Vector3.Cross( ( EndPos - Position).Normalized, Vector3.Up ) * Width;
				var powerFraction = (ShotPower / 100f);

				var a = new Vertex( Position - widthOffset, Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
				var b = new Vertex( Position + widthOffset, Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
				var c = new Vertex( EndPos + widthOffset, Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
				var d = new Vertex( EndPos - widthOffset, Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

				vertexBuffer.AddQuad( a, b, c, d );

				Render.Set( "Opacity", 0.5f + ( ( 0.5f / 100f ) * powerFraction ) );
				Render.Set( "Color", Color );

				vertexBuffer.Draw( LineMaterial );

				vertexBuffer.Clear();

				var circleSize = 1f + (5f * powerFraction);

				a = new Vertex( Position + new Vector3( -circleSize, -circleSize, 0f ), Vector3.Up, Vector3.Right, new Vector4( 0, 1, 0, 0 ) );
				b = new Vertex( Position + new Vector3( circleSize, -circleSize, 0f ), Vector3.Up, Vector3.Right, new Vector4( 1, 1, 0, 0 ) );
				c = new Vertex( Position + new Vector3( circleSize, circleSize, 0f ), Vector3.Up, Vector3.Right, new Vector4( 1, 0, 0, 0 ) );
				d = new Vertex( Position + new Vector3( -circleSize, circleSize, 0f ), Vector3.Up, Vector3.Right, new Vector4( 0, 0, 0, 0 ) );

				vertexBuffer.AddQuad( a, b, c, d );

				Render.Set( "Opacity", powerFraction );

				vertexBuffer.Draw( PowerCircleMaterial );
			}
		}
	}
}
