﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PoolGame
{
	public class LoadingScreen : Panel
	{
		public Label Text;

		public LoadingScreen()
		{
			StyleSheet.Load( "/ui/LoadingScreen.scss" );

			Text = Add.Label( "Loading", "loading" );
		}

		public override void Tick()
		{
			if ( Game.Instance == null ) return;
			
			var isHidden = true;
			var round = Game.Instance.Round;

			if ( round is PlayRound )
			{
				var playerOne = Game.Instance.PlayerOne;
				var playerTwo = Game.Instance.PlayerTwo;

				if ( !playerOne.IsValid || !playerTwo.IsValid )
					isHidden = false;

				if ( !Game.Instance.Cue.IsValid )
					isHidden = false;
			}

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
