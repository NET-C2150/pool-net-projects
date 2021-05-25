
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PoolGame
{
	public class FastForward : Panel
	{
		public Panel Image;

		public FastForward()
		{
			StyleSheet.Load( "/ui/FastForward.scss" );
			Image = Add.Panel( "image" );
		}
	}
}
