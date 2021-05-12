
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace PoolGame
{
	public class BallTrayItem : Panel
	{
		public string IconClass { get; private set; }
		public Panel Icon { get; private set; }

		public BallTrayItem()
		{
			Icon = Add.Panel( "icon" );
		}

		public void Update( PoolBall ball )
		{
			if ( ball == null )
			{
				SetIconClass( "none" );
				return;
			}

			SetIconClass( ball.GetIconClass() );
		}

		public void SetIconClass( string iconClass )
		{
			if ( !string.IsNullOrEmpty( IconClass ) )
				Icon.RemoveClass( IconClass );

			Icon.AddClass( iconClass );
		}
	}

	public class BallTray : Panel
	{
		public static BallTray Current { get; set; }

		public List<BallTrayItem> Items { get; private set; }
		public int Index { get; private set; }

		public BallTray()
		{
			StyleSheet.Load( "/ui/BallTray.scss" );

			Current = this;
			Index = 0;
			Items = new List<BallTrayItem>();

			for ( var i = 0; i < 15; i++ )
			{
				var item = AddChild<BallTrayItem>();
				item.Update( null );
				Items.Add( item );
			}
		}

		[Event("ball.potted")]
		public void AddBall( PoolBall ball )
		{
			if ( Index < Items.Count )
			{
				Items[Index].Update( ball );
				Index++;
			}
		}

		public void Clear()
		{
			foreach ( var item in Items )
				item.Update( null );

			Index = 0;
		}

		public override void Tick()
		{
			var player = Sandbox.Player.Local;
			if ( player == null ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			var round = game.Round;

			if ( round is not PlayRound )
			{
				SetClass( "hidden", true );
				return;
			}

			SetClass( "hidden", false );

			base.Tick();
		}
	}
}
