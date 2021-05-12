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

	public struct EloScore
	{
		public int Rating { get; set; }

		public void Update( EloScore opponent, EloOutcome outcome )
		{
			var eloK = 32;
			var delta = (int)(eloK * ((int)outcome - Elo.GetWinChance( this, opponent )));

			Rating += delta;
			opponent.Rating -= delta;
		}
	}
}
