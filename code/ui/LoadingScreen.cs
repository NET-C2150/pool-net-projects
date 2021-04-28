
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
			var isHidden = false;

			if ( Sandbox.Player.Local is Player player )
			{
				var whiteBall = Game.Instance.WhiteBall;
				var cue = player.Cue;

				isHidden = (whiteBall != null && cue != null);
			}

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
