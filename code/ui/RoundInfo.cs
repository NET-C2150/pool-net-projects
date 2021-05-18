﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PoolGame
{
	public class RoundInfo : Panel
	{
		public Panel Container;
		public Label RoundName;

		public RoundInfo()
		{
			StyleSheet.Load( "/ui/RoundInfo.scss" );

			Container = Add.Panel( "container" );
			RoundName = Container.Add.Label( "Round", "roundName" );
		}

		public override void Tick()
		{
			var player = Local.Pawn as Player;
			if ( player == null ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			var round = game.Round;
			if ( round == null ) return;

			if ( !round.ShowRoundInfo )
			{
				SetClass( "hidden", true );
				return;
			}
			else
			{
				SetClass( "hidden", false );
			}

			RoundName.Text = round.RoundName;
		}
	}
}
