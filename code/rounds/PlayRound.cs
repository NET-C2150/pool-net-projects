using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
    public class PlayRound : BaseRound
	{
		public override string RoundName => "PLAY";
		public override int RoundDuration => 0;
		public override bool CanPlayerSuicide => false;
		public override bool ShowTimeLeft => true;

		public List<Player> Spectators = new();
		public float PlayerTurnEndTime { get; set; }

		public override void OnPlayerLeave( Player player )
		{
			base.OnPlayerLeave( player );

			var playerOne = Game.Instance.PlayerOne;
			var playerTwo = Game.Instance.PlayerTwo;

			if ( player == playerOne || player == playerTwo )
			{
				_ = LoadStatsRound( "Game Over" );
			}
		}

		public override void OnPlayerSpawn( Player player )
		{
			Spectators.Add( player );

			base.OnPlayerSpawn( player );
		}

		public override void OnBallEnterPocket( PoolBall ball, TriggerBallPocket pocket )
		{
			if ( Host.IsServer && ball.LastStriker != null && ball.LastStriker.IsValid() )
			{
				if ( ball.Type == PoolBallType.White )
				{
					ball.LastStriker.Foul( FoulReason.PotWhiteBall );
					Game.Instance.RespawnWhiteBall();
				}
				else if ( ball.Type == ball.LastStriker.BallType || ball.LastStriker.BallType == PoolBallType.White )
				{
					if ( ball.LastStriker.BallType == PoolBallType.White )
					{
						// This is our ball type now, we've claimed it.
						ball.LastStriker.BallType = ball.Type;

						var otherPlayer = Game.Instance.GetOtherPlayer( ball.LastStriker );
						otherPlayer.BallType = (ball.Type == PoolBallType.Red ? PoolBallType.Yellow : PoolBallType.Red);
					}

					Game.Instance.RemoveBall( ball );
					ball.LastStriker.Score++;
				}
				else if ( ball.Type == PoolBallType.Black )
				{
					Game.Instance.RemoveBall( ball );

					if ( ball.LastStriker.BallsLeft == 0 )
					{
						DoPlayerWin( ball.LastStriker );
					}
					else
					{
						DoPlayerWin( Game.Instance.GetOtherPlayer( ball.LastStriker ) );
					}
				}
				else
				{
					Game.Instance.RemoveBall( ball );

					// We get to pot another player's ball in our first shot after a foul.
					if ( !ball.LastStriker.HasSecondShot )
					{
						ball.LastStriker.Foul( FoulReason.PotOtherBall );
					}

					var otherPlayer = Game.Instance.GetOtherPlayer( ball.LastStriker );

					// Let's be sure it's the other player's ball type before we give them score.
					if ( otherPlayer.BallType == ball.Type )
					{
						otherPlayer.Score++;
					}
				}
			}
		}

		public override void OnBallHitOtherBall( PoolBall ball, PoolBall other )
		{
			// Is this the first ball this striker has hit?
			if ( Host.IsServer && ball.Type == PoolBallType.White )
			{
				if ( ball.LastStriker.BallType == PoolBallType.White )
				{
					if ( other.Type == PoolBallType.Black )
					{
						// The player has somehow hit the black as their first strike.
						ball.LastStriker.Foul( FoulReason.HitOtherBall );
					}
				}
				else if ( other.Type == PoolBallType.Black )
				{
					if ( ball.LastStriker.BallsLeft > 0 )
					{
						ball.LastStriker.Foul( FoulReason.HitOtherBall );
					}
				}
				else if ( other.Type != ball.LastStriker.BallType )
				{
					// We get to hit another player's ball in our first shot after a foul.
					if ( !ball.LastStriker.HasSecondShot )
					{
						ball.LastStriker.Foul( FoulReason.HitOtherBall );
					}
				}
			}
		}

		public override void OnSecond()
		{
			if ( Host.IsServer )
			{
				if ( PlayerTurnEndTime > 0f && Sandbox.Time.Now >= PlayerTurnEndTime )
				{
					var currentPlayer = Game.Instance.CurrentPlayer;

					if ( currentPlayer.IsValid )
						currentPlayer.Entity.IsFollowingBall = true;

					return;
				}

				var timeLeft = MathF.Max( PlayerTurnEndTime - Sandbox.Time.Now, 0f );

				TimeLeftFormatted = TimeSpan.FromSeconds( timeLeft ).ToString( @"mm\:ss" );
				NetworkDirty( "TimeLeftFormatted", NetVarGroup.Net );
			}
		}

		public override void OnTick()
		{
			if ( Host.IsServer && Game.Instance != null )
			{
				var currentPlayer = Game.Instance.CurrentPlayer;

				if ( currentPlayer.IsValid && currentPlayer.Entity.IsFollowingBall )
					CheckForStoppedBalls();
			}

			base.OnTick();
		}

		protected override void OnStart()
		{
			Log.Info( "Started Play Round" );

			if ( Host.IsServer )
			{
				Game.Instance.RespawnAllBalls();

				var potentials = new List<Player>();

				Sandbox.Player.All.ForEach( ( v ) =>
				{
					if ( v is Player player )
						potentials.Add( player );
				} );

				var previousWinner = Game.Instance.PreviousWinner;
				var previousLoser = Game.Instance.PreviousLoser;

				if ( previousLoser != null && previousLoser.IsValid() )
				{
					if ( potentials.Count > 2 )
					{
						// Winner stays on, don't let losers play twice.
						potentials.Remove( previousLoser );
					}
				}

				var playerOne = previousWinner;

				if ( playerOne == null || !playerOne.IsValid()) 
					playerOne = potentials[Rand.Int( 0, potentials.Count - 1 )];

				potentials.Remove( playerOne );

				var playerTwo = playerOne;
				
				if ( potentials.Count > 0 )
					playerTwo = potentials[Rand.Int( 0, potentials.Count - 1 )];

				potentials.Remove( playerTwo );

				AddPlayer( playerOne );
				AddPlayer( playerTwo );

				playerOne.StartPlaying();
				playerTwo.StartPlaying();

				if ( Rand.Float( 1f ) >= 0.5f )
					playerOne.StartTurn();
				else
					playerTwo.StartTurn();

				Game.Instance.PlayerOne = playerOne;
				Game.Instance.PlayerTwo = playerTwo;

				// Everyone else is a spectator.
				potentials.ForEach( ( player ) =>
				{
					player.MakeSpectator( true );
					Spectators.Add( player );
				} );

				PlayerTurnEndTime = Sandbox.Time.Now + 30f;
			}
		}

		protected override void OnFinish()
		{
			Log.Info( "Finished Play Round" );

			if ( Host.IsServer )
			{
				var playerOne = Game.Instance.PlayerOne.Entity;
				var playerTwo = Game.Instance.PlayerTwo.Entity;

				playerOne?.MakeSpectator( true );
				playerTwo?.MakeSpectator( true );

				Spectators.Clear();
			}
		}

		private async Task LoadStatsRound(string title = "", int delay = 3)
		{
			await Task.Delay( delay * 1000 );

			if ( Game.Instance.Round != this )
				return;

			Game.Instance.ChangeRound( new StatsRound() );
		}

		private void DoPlayerWin( Player player )
		{
			Log.Info( player.Name + " has won the game!" );

			_ = LoadStatsRound( $"{player.Name} Wins" );
		}

		private void CheckForStoppedBalls()
		{
			foreach ( var ball in Game.Instance.AllBalls )
			{
				// Is this a shit way of determining it?
				if ( ball.PhysicsBody.Velocity.Length > 5f )
					return;

				if ( ball.PhysicsBody.AngularVelocity.Length > 5f )
					return;
			}

			var currentPlayer = Game.Instance.CurrentPlayer.Entity;
			var otherPlayer = Game.Instance.GetOtherPlayer( currentPlayer );

			if ( !currentPlayer.HasSecondShot )
			{
				currentPlayer.FinishTurn();
				otherPlayer.StartTurn( currentPlayer.FoulReason != FoulReason.None );
			}
			else
			{
				currentPlayer.StartTurn();
			}

			PlayerTurnEndTime = Sandbox.Time.Now + 30f;
		}
	}
}
