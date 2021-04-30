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
}
