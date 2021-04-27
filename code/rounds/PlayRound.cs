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
		public override string RoundName => "RACE";
		public override int RoundDuration => 300;
		public override bool CanPlayerSuicide => true;

		public List<Player> Spectators = new();

		private bool _isGameOver;

		public override void OnPlayerKilled( Player player )
		{
			Players.Remove( player );
			Spectators.Add( player );

			if ( Players.Count <= 1 )
			{
				_ = LoadStatsRound( "Game Over" );
			}
		}

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
			Players.Remove( player );

			base.OnPlayerSpawn( player );
		}

		protected override void OnStart()
		{
			Log.Info( "Started Play Round" );

			if ( Host.IsServer )
			{
				Sandbox.Player.All.ForEach( ( player ) => AddPlayer( player as Player ) );
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
	}
}
