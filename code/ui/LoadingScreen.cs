
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
			
			var isHidden = (Local.Pawn is Player player && player.Camera != null);
			var round = Game.Instance.Round;

			if ( round is PlayRound )
			{
				var playerOne = Game.Instance.PlayerOne;
				var playerTwo = Game.Instance.PlayerTwo;
				var cue = Game.Instance.Cue;

				if ( !playerOne.IsValid() || !playerTwo.IsValid() || !cue.IsValid() )
					isHidden = false;
			}

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
