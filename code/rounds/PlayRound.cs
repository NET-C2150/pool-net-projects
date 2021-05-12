using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
    public partial class PlayRound : BaseRound
	{
		public struct PotHistoryItem
		{
			public PoolBallNumber Number;
			public PoolBallType Type;
		}

		public override string RoundName => "PLAY";
		public override int RoundDuration => 0;
		public override bool CanPlayerSuicide => false;
		public override bool ShowTimeLeft => true;

		public List<Player> Spectators = new();
		public float PlayerTurnEndTime { get; set; }

		[Net] public NetworkList<PotHistoryItem> PotHistory { get; set; }

		public PlayRound()
		{
			PotHistory = new();

			if ( Host.IsClient )
			{
				PotHistory.OnListUpdated += OnPotHistoryUpdated;
			}
		}

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
			if ( Host.IsServer )
			{
				ball.PlaySound( $"ball-pocket-{Rand.Int( 1, 2 )}" );

				if ( ball.LastStriker == null || !ball.LastStriker.IsValid() )
				{
					if ( ball.Type == PoolBallType.White )
					{
						_ = Game.Instance.RespawnBallAsync( ball, true );
					}
					else if ( ball.Type == PoolBallType.Black )
					{
						_ = Game.Instance.RespawnBallAsync( ball, true );
					}
					else
					{
						var player = Game.Instance.GetBallPlayer( ball );

						if ( player != null && player.IsValid() )
						{
							if ( Game.Instance.CurrentPlayer == player )
								player.HasSecondShot = true;

							DoPlayerPotBall( null, ball, BallPotType.Silent );
						}

						_ = Game.Instance.RemoveBallAsync( ball, true );
					}

					return;
				}

				if ( ball.Type == PoolBallType.White )
				{
					ball.LastStriker.Foul( FoulReason.PotWhiteBall );
					_ = Game.Instance.RespawnBallAsync( ball, true );
				}
				else if ( ball.Type == ball.LastStriker.BallType )
				{
					if ( Game.Instance.CurrentPlayer == ball.LastStriker )
						ball.LastStriker.HasSecondShot = true;

					DoPlayerPotBall( ball.LastStriker, ball, BallPotType.Normal );

					_ = Game.Instance.RemoveBallAsync( ball, true );
				}
				else if ( ball.Type == PoolBallType.Black )
				{
					DoPlayerPotBall( ball.LastStriker, ball, BallPotType.Normal );

					_ = Game.Instance.RemoveBallAsync( ball, true );

					if ( ball.LastStriker.BallsLeft == 0 )
						DoPlayerWin( ball.LastStriker );
					else
						DoPlayerWin( Game.Instance.GetOtherPlayer( ball.LastStriker ) );
				}
				else
				{
					if ( ball.LastStriker.BallType == PoolBallType.White )
					{
						// This is our ball type now, we've claimed it.
						ball.LastStriker.HasSecondShot = true;
						ball.LastStriker.BallType = ball.Type;

						var otherPlayer = Game.Instance.GetOtherPlayer( ball.LastStriker );
						otherPlayer.BallType = (ball.Type == PoolBallType.Spots ? PoolBallType.Stripes : PoolBallType.Spots);

						DoPlayerPotBall( ball.LastStriker, ball, BallPotType.Claim );
					}
					else
					{
						// We get to pot another player's ball in our first shot after a foul.
						if ( !ball.LastStriker.HasSecondShot )
							ball.LastStriker.Foul( FoulReason.PotOtherBall );

						DoPlayerPotBall( ball.LastStriker, ball, BallPotType.Normal );
					}

					_ = Game.Instance.RemoveBallAsync( ball, true );
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
				if ( PlayerTurnEndTime > 0f && Time.Now >= PlayerTurnEndTime )
				{
					var currentPlayer = Game.Instance.CurrentPlayer;

					if ( currentPlayer.IsValid )
						currentPlayer.Entity.IsFollowingBall = true;

					return;
				}

				var timeLeft = MathF.Max( PlayerTurnEndTime - Time.Now, 0f );
				TimeLeftSeconds = timeLeft.CeilToInt();
				NetworkDirty( "TimeLeftSeconds", NetVarGroup.Net );
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
			else
			{
				BallHistory.Current.Clear();
			}
		}

		private async Task LoadStatsRound(string title = "", int delay = 3)
		{
			await Task.Delay( delay * 1000 );

			if ( Game.Instance.Round != this )
				return;

			Game.Instance.ChangeRound( new StatsRound() );
		}

		private void DoPlayerPotBall( Player player, PoolBall ball, BallPotType type )
		{
			player.DidPotBall = true;

			PotHistory.Add( new PotHistoryItem
			{
				Type = ball.Type,
				Number = ball.Number
			} );

			if ( type == BallPotType.Normal )
				Game.Instance.AddToast( player, $"{ player.Name } has potted a ball", ball.GetIconClass() );
			else if ( type == BallPotType.Claim )
				Game.Instance.AddToast( player, $"{ player.Name } has claimed { ball.Type }", ball.GetIconClass() );

			var owner = Game.Instance.GetBallPlayer( ball );

			if ( owner != null && owner.IsValid() )
				owner.Score++;
		}

		private void DoPlayerWin( Player player )
		{
			Game.Instance.AddToast( player, $"{ player.Name} has won the game", "wins" );

			var otherPlayer = Game.Instance.GetOtherPlayer( player );
			player.Elo.Update( otherPlayer.Elo, EloOutcome.Win );

			_ = LoadStatsRound( $"{player.Name} Wins" );
		}

		private void OnPotHistoryUpdated()
		{
			BallHistory.Current.Clear();

			foreach ( var item in PotHistory.Values )
				BallHistory.Current.AddByType( item.Type, item.Number );
		}

		private void CheckForStoppedBalls()
		{
			foreach ( var ball in Game.Instance.AllBalls )
			{
				if ( !ball.PhysicsBody.Velocity.IsNearlyZero( 0.01f ) )
					return;

				if ( ball.IsAnimating )
					return;
			}

			var currentPlayer = Game.Instance.CurrentPlayer.Entity;
			var didHitAnyBall = currentPlayer.DidPotBall;

			if ( !didHitAnyBall )
			{
				foreach ( var ball in Game.Instance.AllBalls )
				{
					if ( ball.Type != PoolBallType.White && ball.LastStriker == currentPlayer )
					{
						didHitAnyBall = true;
						break;
					}
				}
			}

			foreach ( var ball in Game.Instance.AllBalls )
				ball.ResetLastStriker();

			if ( !didHitAnyBall )
				currentPlayer.Foul( FoulReason.HitNothing );

			if ( currentPlayer.IsPlacingWhiteBall )
				currentPlayer.StopPlacingWhiteBall();

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
