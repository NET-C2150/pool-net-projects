using System;
using System.Threading.Tasks;
using Sandbox;

namespace PoolGame
{
	public class DoubleOnBlack : Rule
	{
		public override string Name => "Double On Black";
		public override string Description => "You must bounce the white off the side before you hit the black ball.";
	}
}
