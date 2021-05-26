using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace PoolGame
{
	public partial class Ruleset : Entity
	{
		[Net] public DoubleOnBlack DoubleOnBlack { get; set; }
		[Net] public WinnerStaysOn WinnerStaysOn { get; set; }

		[Net] public List<Rule> Available { get; set; }

		public Ruleset()
		{
			Available = new List<Rule>();

			if ( IsServer )
			{
				DoubleOnBlack = new();
				Available.Add( DoubleOnBlack );

				WinnerStaysOn = new();
				Available.Add( WinnerStaysOn );
			}

			Transmit = TransmitType.Always;
		}
	}
}
