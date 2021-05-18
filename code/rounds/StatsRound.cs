using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public partial class StatsRound : BaseRound
	{
		public override string RoundName => "STATS";
		public override int RoundDuration => 10;

		protected override void OnStart()
		{
			Log.Info( "Started Stats Round" );
		}

		protected override void OnFinish()
		{
			Log.Info( "Finished Stats Round" );
		}

		protected override void OnTimeUp()
		{
			Log.Info( "Stats Time Up!" );

			Game.Instance.ChangeRound( new PlayRound() );

			base.OnTimeUp();
		}

		public override void OnPlayerJoin( Player player )
		{
			if ( Players.Contains( player ) ) return;

			AddPlayer( player );

			base.OnPlayerJoin( player );
		}
	}
}
