
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PoolGame
{
	public class PlayerDisplayItem : Panel
	{
		public Label Name;
		public Image Avatar;
		public Label Score;

		public PlayerDisplayItem()
		{
			Name = Add.Label( "Name", "name" );
			Avatar = Add.Image( "", "avatar" );
			Score = Add.Label( "0", "score" );
		}

		public void Update( EntityHandle<Player> player )
		{
			var isValid = player.IsValid;

			if ( isValid )
			{
				Name.Text = player.Entity.Name;
				Score.Text = player.Entity.Score.ToString();
				Avatar.SetTexture( $"avatar:{player.Entity.SteamId}" );

				SetClass( "active", player.Entity.IsTurn );
			}

			SetClass( "hidden", !isValid );
		}
	}

	public class PlayerDisplay : Panel
	{
		public PlayerDisplayItem PlayerOne;
		public PlayerDisplayItem PlayerTwo;

		public PlayerDisplay()
		{
			StyleSheet.Load( "/ui/PlayerDisplay.scss" );

			PlayerOne = AddChild<PlayerDisplayItem>( "one" );
			PlayerTwo = AddChild<PlayerDisplayItem>( "two" );
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

			PlayerOne.Update( Game.Instance.PlayerOne );
			PlayerTwo.Update( Game.Instance.PlayerTwo );

			SetClass( "hidden", false );
		}
	}
}
