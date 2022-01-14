using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Pool
{
	public enum EloOutcome
	{
		Loss = 0,
		Win = 1
	}

	public partial class EloScore : BaseNetworkable
	{
		[Net] public int Rating { get; set; }
		[Net] public int Delta { get; set; }

		public PlayerRank GetRank()
		{
			return Elo.GetRank( Rating );
		}

		public PlayerRank GetNextRank()
		{
			return Elo.GetNextRank( Rating );
		}

		public int GetLevel()
		{
			return Elo.GetLevel( Rating );
		}

		public void Update( EloScore opponent, EloOutcome outcome )
		{
			var eloK = 32;
			var delta = (int)(eloK * ((int)outcome - Elo.GetWinChance( this, opponent )));

			Rating = Math.Max( Rating + delta, 0 );
			Delta = delta;

			opponent.Rating = Math.Max( opponent.Rating - delta, 0 );
			opponent.Delta = Delta;
		}
	}
}
