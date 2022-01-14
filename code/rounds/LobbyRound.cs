using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Pool
{
    public class LobbyRound : BaseRound
	{
		public override string RoundName => "LOBBY";

		protected override void OnStart()
		{
			Log.Info( "Started Lobby Round" );

			if ( Host.IsServer )
			{
				var players = Client.All.Select( ( client ) => client.Pawn as Player );

				foreach ( var player in players )
					OnPlayerJoin( player );

				Game.Instance.RemoveAllBalls();
			}
		}

		protected override void OnFinish()
		{
			Log.Info( "Finished Lobby Round" );
		}

		public override void OnPlayerJoin( Player player )
		{
			if ( Players.Contains( player ) )
			{
				return;
			}

			player.MakeSpectator( true );

			AddPlayer( player );

			base.OnPlayerJoin( player );
		}
	}
}
