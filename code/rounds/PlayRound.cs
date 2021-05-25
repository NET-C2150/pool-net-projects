﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
    public partial class PlayRound : BaseRound
	{
		public override string RoundName => "PLAY";
		public override int RoundDuration => 0;
		public override bool CanPlayerSuicide => false;
		public override bool ShowTimeLeft => true;

		public List<Player> Spectators = new();
		public float PlayerTurnEndTime { get; set; }
		public bool DidClaimThisTurn { get; private set; }
		public Sound? ClockTickingSound { get; private set; }
		public TimeSince TimeSinceTurnTaken { get; private set; }
		[Net] PoolBall BallLikelyToPot { get; set; }

		public override void OnPlayerLeave( Player player )
		{
			base.OnPlayerLeave( player );

			var playerOne = Game.Instance.PlayerOne;
			var playerTwo = Game.Instance.PlayerTwo;

			if ( player == playerOne || player == playerTwo )
			{
				Game.Instance.ChangeRound( new StatsRound() );
			}
		}

		public override void UpdatePlayerPosition( Player player )
		{
			if ( BallLikelyToPot.IsValid() )
			{
				player.Position = BallLikelyToPot.Position.WithZ( 200f );
			}
			else
			{
				player.Position = new Vector3( 0f, 0f, 350f );
			}
			
			player.Rotation = Rotation.LookAt( Vector3.Down );
		}

		public override void OnPlayerJoin( Player player )
		{
			Spectators.Add( player );

			base.OnPlayerJoin( player );
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
							var currentPlayer = Game.Instance.CurrentPlayer;

							if ( currentPlayer == player )
								player.HasSecondShot = true;

							DoPlayerPotBall( currentPlayer, ball, BallPotType.Silent );
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
					{
						ball.LastStriker.HasSecondShot = true;
						ball.LastStriker.DidHitOwnBall = true;
					}

					DoPlayerPotBall( ball.LastStriker, ball, BallPotType.Normal );

					_ = Game.Instance.RemoveBallAsync( ball, true );
				}
				else if ( ball.Type == PoolBallType.Black )
				{
					DoPlayerPotBall( ball.LastStriker, ball, BallPotType.Normal );

					_ = Game.Instance.RemoveBallAsync( ball, true );
				}
				else
				{
					if ( ball.LastStriker.BallType == PoolBallType.White )
					{
						// This is our ball type now, we've claimed it.
						ball.LastStriker.DidHitOwnBall = true;
						ball.LastStriker.HasSecondShot = true;
						ball.LastStriker.BallType = ball.Type;

						var otherPlayer = Game.Instance.GetOtherPlayer( ball.LastStriker );
						otherPlayer.BallType = (ball.Type == PoolBallType.Spots ? PoolBallType.Stripes : PoolBallType.Spots);

						DoPlayerPotBall( ball.LastStriker, ball, BallPotType.Claim );

						DidClaimThisTurn = true;
					}
					else
					{
						if ( !DidClaimThisTurn )
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
					if ( ball.LastStriker.BallsLeft > 0 && !ball.LastStriker.DidHitOwnBall )
					{
						ball.LastStriker.Foul( FoulReason.HitOtherBall );
					}
				}
				else if ( other.Type != ball.LastStriker.BallType )
				{
					if ( !ball.LastStriker.DidHitOwnBall )
						ball.LastStriker.Foul( FoulReason.HitOtherBall );
				}
				else if ( ball.LastStriker.FoulReason == FoulReason.None )
				{
					ball.LastStriker.DidHitOwnBall = true;
				}
			}
		}

		public override void OnSecond()
		{
			if ( Host.IsServer )
			{
				var currentPlayer = Game.Instance.CurrentPlayer;

				if ( currentPlayer == null || !currentPlayer.IsValid() )
					return;

				if ( PlayerTurnEndTime > 0f && Time.Now >= PlayerTurnEndTime )
					currentPlayer.HasStruckWhiteBall = true;

				if ( currentPlayer.HasStruckWhiteBall )
					return;

				var timeLeft = MathF.Max( PlayerTurnEndTime - Time.Now, 0f );
				TimeLeftSeconds = timeLeft.CeilToInt();

				if ( timeLeft <= 4f && ClockTickingSound == null )
				{
					ClockTickingSound = currentPlayer.PlaySound( "clock-ticking" );
					ClockTickingSound.Value.SetVolume( 0.5f );
				}
			}
		}

		public override void OnTick()
		{
			if ( Host.IsServer && Game.Instance != null )
			{
				var currentPlayer = Game.Instance.CurrentPlayer;

				if ( currentPlayer != null && currentPlayer.IsValid() && currentPlayer.HasStruckWhiteBall )
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
				var players = Client.All.Select( ( client ) => client.Pawn as Player );

				foreach ( var player in players )
					potentials.Add( player );

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
				Game.Instance.PotHistory.Clear();

				var playerOne = Game.Instance.PlayerOne;
				var playerTwo = Game.Instance.PlayerTwo;

				playerOne?.MakeSpectator( true );
				playerTwo?.MakeSpectator( true );

				Spectators.Clear();
			}
		}

		private void DoPlayerPotBall( Player player, PoolBall ball, BallPotType type )
		{
			player.DidPotBall = true;

			Game.Instance.PotHistory.Add( new PotHistoryItem
			{
				Type = ball.Type,
				Number = ball.Number
			} );

			var client = player.GetClientOwner();

			if ( type == BallPotType.Normal )
				Game.Instance.AddToast( To.Everyone, player, $"{ client.Name } has potted a ball", ball.GetIconClass() );
			else if ( type == BallPotType.Claim )
				Game.Instance.AddToast( To.Everyone, player, $"{ client.Name } has claimed { ball.Type }", ball.GetIconClass() );

			var owner = Game.Instance.GetBallPlayer( ball );

			if ( owner != null && owner.IsValid() )
				owner.Score++;
		}

		private void DoPlayerWin( Player winner )
		{
			var client = winner.GetClientOwner();

			Game.Instance.AddToast( To.Everyone, winner, $"{ client.Name } has won the game", "wins" );

			var loser = Game.Instance.GetOtherPlayer( winner );
			winner.Elo.Update( loser.Elo, EloOutcome.Win );

			foreach ( var c in Entity.All.OfType<Player>() )
			{
				if ( c == loser )
					c.SendSound( To.Single( c ), "lose-game" );
				else
					c.SendSound( To.Single( c ), "win-game" );
			}

			Game.Instance.UpdateRating( winner );
			Game.Instance.UpdateRating( loser );
			Game.Instance.SaveRatings();

			Game.Instance.ChangeRound( new StatsRound() );
		}

		private PoolBall FindBallLikelyToPot()
		{
			var currentPlayer = Game.Instance.CurrentPlayer;
			var potentials = Game.Instance.AllBalls;
			var pockets = Entity.All.OfType<TriggerBallPocket>();

			foreach ( var ball in potentials )
			{
				if ( ball.PhysicsBody.Velocity.Length < 2f )
					continue;

				var fromTransform = ball.PhysicsBody.Transform;
				var toTransform = ball.PhysicsBody.Transform;
				toTransform.Position = ball.Position + ball.PhysicsBody.Velocity * 3f;

				var sweep = Trace.Sweep( ball.PhysicsBody, fromTransform, toTransform )
					.Ignore( ball )
					.Run();

				if ( sweep.Entity is PoolBall )
					continue;

				foreach ( var pocket in pockets )
				{
					if ( pocket.Position.Distance( sweep.EndPos ) <= 5f )
						return ball;

					if ( ball.Position.Distance( pocket.Position ) <= 5f )
						return ball;
				}
			}

			return null;
		}

		private void CheckForStoppedBalls()
		{
			var currentPlayer = Game.Instance.CurrentPlayer;

			if ( currentPlayer.TimeSinceWhiteStruck >= 2f && !BallLikelyToPot.IsValid() )
			{
				BallLikelyToPot = FindBallLikelyToPot();

				if ( BallLikelyToPot.IsValid() )
					currentPlayer.PlaySound( $"gasp-{Rand.Int( 1, 2 )}" );
			}

			if ( currentPlayer.TimeSinceWhiteStruck >= 3f && !BallLikelyToPot.IsValid() )
				Global.PhysicsTimeScale = 5f;

			// Now check if all balls are essentially still.
			foreach ( var ball in Game.Instance.AllBalls )
			{
				if ( !ball.PhysicsBody.Velocity.IsNearlyZero( 0.2f ) )
					return;

				if ( ball.IsAnimating )
					return;
			}

			Game.Instance.AllBalls.ForEach( ( ball ) =>
			{
				ball.PhysicsBody.AngularVelocity = Vector3.Zero;
				ball.PhysicsBody.Velocity = Vector3.Zero;
				ball.PhysicsBody.ClearForces();
			} );

			Game.Instance.Cue.Reset();

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
			var blackBall = Game.Instance.BlackBall;

			if ( blackBall == null || !blackBall.IsValid() )
			{
				if ( currentPlayer.FoulReason == FoulReason.None )
				{
					if ( currentPlayer.BallsLeft == 0 )
						DoPlayerWin( currentPlayer );
					else
						DoPlayerWin( otherPlayer );
				}
				else
				{
					DoPlayerWin( otherPlayer );
				}
			}
			else
			{
				if ( !currentPlayer.HasSecondShot )
				{
					currentPlayer.FinishTurn();
					otherPlayer.StartTurn( currentPlayer.FoulReason != FoulReason.None );
				}
				else
				{
					currentPlayer.StartTurn( false, false );
				}
			}

			if ( ClockTickingSound != null )
			{
				ClockTickingSound.Value.Stop();
				ClockTickingSound = null;
			}

			Global.PhysicsTimeScale = 1f;

			PlayerTurnEndTime = Time.Now + 30f;
			DidClaimThisTurn = false;
			BallLikelyToPot = null;
		}
	}
}
