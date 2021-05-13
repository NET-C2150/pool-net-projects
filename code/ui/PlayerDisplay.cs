
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PoolGame
{
	public class PlayerDisplayItem : Panel
	{
		public Panel PlayerContainer;
		public Label Name;
		public Image Avatar;

		public Panel ScoreContainer;
		public Panel BallType;

		public PlayerDisplayItem()
		{
			PlayerContainer = Add.Panel("player-container");
			Avatar = PlayerContainer.Add.Image( "", "avatar" );
			Name = PlayerContainer.Add.Label("Name", "name");

			ScoreContainer = Add.Panel("score-container");
			BallType = ScoreContainer.Add.Panel("ball");

		}

		public void Update( EntityHandle<Player> player )
		{
			var isValid = player.IsValid;

			var game = Game.Instance;
			if ( game == null ) return;
			var round = game.Round;
			if ( round == null ) return;

			if ( isValid )
			{
				Name.Text = player.Entity.Name;

				Avatar.SetTexture( $"avatar:{player.Entity.SteamId}" );

				BallType.SetClass( "spots", player.Entity.BallType == PoolBallType.Spots );
				BallType.SetClass( "stripes", player.Entity.BallType == PoolBallType.Stripes );

				SetClass( "active", player.Entity.IsTurn );	
			}

			SetClass( "hidden", !isValid );
		}
	}

	public class PlayerDisplay : Panel
	{
		public PlayerDisplayItem PlayerOne;
		public PlayerDisplayItem PlayerTwo;

		public Panel TimeRemainingNumber;
		public Label TimeRemainingLabel;

		public Panel TimeBarWrapper;
		public Panel TimeRemainingBar;
		public Panel TimeRemainingProgress;

		public PlayerDisplay()
		{
			StyleSheet.Load( "/ui/PlayerDisplay.scss" );

			PlayerOne = AddChild<PlayerDisplayItem>( "one" );

			TimeRemainingNumber = Add.Panel( "time-remaining-number" );
			TimeRemainingLabel = TimeRemainingNumber.Add.Label( "30", "time-remaining-label" );

			TimeBarWrapper = Add.Panel( "time-bar-wrapper" );
			TimeRemainingBar = TimeBarWrapper.Add.Panel( "time-remaining" );
			TimeRemainingProgress = TimeRemainingBar.Add.Panel( "time-remaining-progress" );

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


			TimeRemainingLabel.Text = round.TimeLeftSeconds.ToString();

			TimeRemainingProgress.Style.Width = Length.Percent( (100f / 30f) * round.TimeLeftSeconds );
			TimeRemainingProgress.Style.Dirty();

			if ( round.TimeLeftSeconds <= 5 )
			{
				TimeBarWrapper.SetClass( "low", true );
				TimeRemainingNumber.SetClass( "low", true );
			}
			else
			{
				TimeBarWrapper.SetClass( "low", false );
				TimeRemainingNumber.SetClass( "low", false );
			}

			SetClass( "hidden", false );
		}
	}
}
