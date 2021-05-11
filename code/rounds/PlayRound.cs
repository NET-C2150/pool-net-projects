﻿using Sandbox;
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

							player.Score++;
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

					Game.Instance.AddToast( ball.LastStriker, $"{ ball.LastStriker.Name} has potted a ball", ball.GetIconClass() );
					ball.LastStriker.Score++;

					_ = Game.Instance.RemoveBallAsync( ball, true );
				}
				else if ( ball.Type == PoolBallType.Black )
				{
					Game.Instance.AddToast( ball.LastStriker, $"{ ball.LastStriker.Name} has potted a ball", ball.GetIconClass() );
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
						Game.Instance.AddToast( ball.LastStriker, $"{ ball.LastStriker.Name} has claimed { ball.Type }", ball.GetIconClass() );

						// This is our ball type now, we've claimed it.
						ball.LastStriker.HasSecondShot = true;
						ball.LastStriker.BallType = ball.Type;
						ball.LastStriker.Score++;

						var otherPlayer = Game.Instance.GetOtherPlayer( ball.LastStriker );
						otherPlayer.BallType = (ball.Type == PoolBallType.Spots ? PoolBallType.Stripes : PoolBallType.Spots);
					}
					else
					{
						// We get to pot another player's ball in our first shot after a foul.
						if ( !ball.LastStriker.HasSecondShot )
							ball.LastStriker.Foul( FoulReason.PotOtherBall );

						var otherPlayer = Game.Instance.GetOtherPlayer( ball.LastStriker );

						// Let's be sure it's the other player's ball type before we give them score.
						if ( otherPlayer.BallType == ball.Type )
						{
							if ( Game.Instance.CurrentPlayer == otherPlayer )
								otherPlayer.HasSecondShot = true;

							Game.Instance.AddToast( ball.LastStriker, $"{ ball.LastStriker.Name} has potted a ball", ball.GetIconClass() );
							otherPlayer.Score++;
						}
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
			Game.Instance.AddToast( player, $"{ player.Name} has won the game", "wins" );

			_ = LoadStatsRound( $"{player.Name} Wins" );
		}

		private void CheckForStoppedBalls()
		{
			foreach ( var ball in Game.Instance.AllBalls )
			{
				// Is this a shit way of determining it?
				if ( ball.PhysicsBody.Velocity.Length > 0.1f )
					return;

				if ( ball.PhysicsBody.AngularVelocity.Length > 0.1f )
					return;

				if ( ball.IsAnimating )
					return;
			}

			var currentPlayer = Game.Instance.CurrentPlayer.Entity;
			var didHitAnyBall = false;

			foreach ( var ball in Game.Instance.AllBalls )
			{
				if ( ball.Type != PoolBallType.White && ball.LastStriker == currentPlayer )
				{
					didHitAnyBall = true;
					break;
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
