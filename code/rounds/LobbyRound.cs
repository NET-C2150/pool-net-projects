using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
    public class LobbyRound : BaseRound
	{
		public override string RoundName => "LOBBY";

		protected override void OnStart()
		{
			Log.Info( "Started Lobby Round" );

			if ( Host.IsServer )
			{
				Sandbox.Player.All.ForEach( ( player ) => (player as Player).Respawn() );

				Game.Instance.RemoveAllBalls();
			}
		}

		protected override void OnFinish()
		{
			Log.Info( "Finished Lobby Round" );
		}

		public override void OnPlayerSpawn( Player player )
		{
			if ( Players.Contains( player ) )
			{
				return;
			}

			player.MakeSpectator( true );

			AddPlayer( player );

			base.OnPlayerSpawn( player );
		}
	}
}
