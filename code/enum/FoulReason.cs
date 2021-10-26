using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public enum FoulReason
	{
		None = 0,
		PotWhiteBall = 1,
		BallLeftTable = 2,
		PotBlackTooEarly = 3,
		HitOtherBall = 4,
		PotOtherBall = 5,
		HitNothing = 6
	}

	internal static class FoulReasonExtension
	{
		public static string ToMessage( this FoulReason reason, Player player )
		{
			var client = player.Client;

			switch ( reason )
			{
				case FoulReason.PotWhiteBall:
					return $"{ client.Name } potted the white ball";
				case FoulReason.BallLeftTable:
					return $"{ client.Name } shot a ball off the table";
				case FoulReason.PotBlackTooEarly:
					return $"{ client.Name } potted the black too early";
				case FoulReason.HitOtherBall:
					return $"{ client.Name } hit the wrong ball";
				case FoulReason.PotOtherBall:
					return $"{ client.Name } potted the wrong ball";
				case FoulReason.HitNothing:
					return $"{ client.Name } didn't hit anything";
				case FoulReason.None:
					break;
			}

			return null;
		}
	}
}
