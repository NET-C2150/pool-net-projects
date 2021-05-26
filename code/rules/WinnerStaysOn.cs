using System;
using System.Threading.Tasks;
using Sandbox;

namespace PoolGame
{
	public class WinnerStaysOn : Rule
	{
		public override string Name => "Winner Stays On";
		public override string Description => "The previous winner keeps playing when the game is over.";
		public override bool EnabledByDefault => true;
	}
}
