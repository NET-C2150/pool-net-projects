
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
		public Label Score;
		public Panel BallType;

		public Panel TimeBarWrapper;
		public Panel TimeRemainingBar;
		public Panel TimeRemainingProgress;

		public Panel TimeRemainingNumber;
		public Label TimeRemainingLabel;

		public PlayerDisplayItem()
		{
			PlayerContainer = Add.Panel("player-container");
			Avatar = PlayerContainer.Add.Image( "", "avatar" );
			Name = PlayerContainer.Add.Label("Name", "name");

			ScoreContainer = Add.Panel("score-container");
			Score = ScoreContainer.Add.Label("0", "score");
			BallType = ScoreContainer.Add.Panel("ball");


			TimeBarWrapper = Add.Panel( "time-bar-wrapper" );

			TimeRemainingBar = TimeBarWrapper.Add.Panel( "time-remaining" );
			TimeRemainingProgress = TimeRemainingBar.Add.Panel( "time-remaining-progress" );

			TimeRemainingNumber = TimeBarWrapper.Add.Panel( "time-remaining-number" );
			TimeRemainingLabel = TimeRemainingNumber.Add.Label( "30", "time-remaining-label" );

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
				Score.Text = player.Entity.Score.ToString();

				Avatar.SetTexture( $"avatar:{player.Entity.SteamId}" );

				BallType.SetClass( "spots", player.Entity.BallType == PoolBallType.Spots );
				BallType.SetClass( "stripes", player.Entity.BallType == PoolBallType.Stripes );

				SetClass( "active", player.Entity.IsTurn );


				TimeRemainingProgress.Style.Width = Length.Percent( (100f / 30f) * round.TimeLeftSeconds );
				TimeRemainingProgress.Style.Dirty();

				if ( round.TimeLeftSeconds <= 5 )
				{
					TimeBarWrapper.SetClass( "low", true );
				}
				else
				{
					TimeBarWrapper.SetClass( "low", false );
				}

				TimeRemainingLabel.Text = round.TimeLeftSeconds.ToString();

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
