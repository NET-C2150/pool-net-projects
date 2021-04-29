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
		public override int RoundDuration => 300;
		public override bool CanPlayerSuicide => false;

		public List<Player> Spectators = new();

		private bool _isGameOver;

		public override void OnPlayerLeave( Player player )
		{
			base.OnPlayerLeave( player );

			Spectators.Remove( player );

			if ( Players.Count <= 1 )
			{
				_ = LoadStatsRound( "Game Over" );
			}
		}

		public override void OnPlayerSpawn( Player player )
		{
			Spectators.Add( player );

			base.OnPlayerSpawn( player );
		}

		public override void OnTick()
		{
			if ( Host.IsServer )
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
			}
		}

		protected override void OnFinish()
		{
			Log.Info( "Finished Play Round" );

			if ( Host.IsServer )
			{
				Spectators.Clear();
			}
		}

		protected override void OnTimeUp()
		{
			if ( _isGameOver ) return;

			Log.Info( "Play Time Up!" );

			_ = LoadStatsRound();

			base.OnTimeUp();
		}

		private async Task LoadStatsRound(string title = "", int delay = 3)
		{
			_isGameOver = true;

			await Task.Delay( delay * 1000 );

			if ( Game.Instance.Round != this )
				return;

			Game.Instance.ChangeRound( new StatsRound() );
		}

		private void CheckForStoppedBalls()
		{
			foreach ( var ball in Game.Instance.AllBalls )
			{
				if ( ball.PhysicsBody.Velocity.Length > 10f )
					return;
			}

			var currentPlayer = Game.Instance.CurrentPlayer;
			var playerOne = Game.Instance.PlayerOne;
			var playerTwo = Game.Instance.PlayerTwo;

			currentPlayer.Entity.FinishTurn();

			if ( currentPlayer == playerOne )
				playerTwo.Entity.StartTurn();
			else
				playerOne.Entity.StartTurn();
		}
	}
}
