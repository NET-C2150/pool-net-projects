using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Pool
{
	public static class Elo
	{
		public static int GetNextLevelRating( int rating )
		{
			var roundedUp = Math.Max( ((int)MathF.Ceiling( rating / 100 ) * 100) - 1, 0 );
			return rating == roundedUp ? rating + 100 : roundedUp;
		}

		public static float GetWinChance( EloScore one, EloScore two )
		{
			return 1f / (1f + MathF.Pow( 10f, (two.Rating - one.Rating) / 400f ));
		}

		public static PlayerRank GetRank( int rating )
		{
			if ( rating < 1149 )
				return PlayerRank.Bronze;
			else if ( rating < 1499 )
				return PlayerRank.Silver;
			else if ( rating < 1849 )
				return PlayerRank.Gold;
			else if ( rating < 2199 )
				return PlayerRank.Platinum;
			else
				return PlayerRank.Diamond;
		}

		public static PlayerRank GetNextRank( int rating )
		{
			var rank = GetRank( rating );

			if ( rank == PlayerRank.Bronze )
				return PlayerRank.Silver;
			else if ( rank == PlayerRank.Silver )
				return PlayerRank.Gold;
			else if ( rank == PlayerRank.Gold )
				return PlayerRank.Platinum;
			else
				return PlayerRank.Diamond;
		}

		public static int GetLevel( int rating )
		{
			return rating / 100;
		}
	}
}
