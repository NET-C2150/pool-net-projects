using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public enum EloOutcome
	{
		Loss = 0,
		Win = 1
	}

	public partial class EloScore : NetworkComponent
	{
		[Net] public int Rating { get; set; }

		public PlayerRank GetRank()
		{
			if ( Rating < 1149 )
				return PlayerRank.Bronze;
			else if ( Rating < 1499 )
				return PlayerRank.Silver;
			else if ( Rating < 1849 )
				return PlayerRank.Gold;
			else if ( Rating < 2199 )
				return PlayerRank.Platinum;
			else
				return PlayerRank.Diamond;
		}

		public int GetLevel()
		{
			return Rating / 100;
		}

		public void Update( EloScore opponent, EloOutcome outcome )
		{
			var eloK = 32;
			var delta = (int)(eloK * ((int)outcome - Elo.GetWinChance( this, opponent )));

			Rating += delta;
			opponent.Rating -= delta;
		}
	}
}
