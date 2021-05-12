
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library]
	public partial class Hud : Sandbox.Hud
	{
		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<RoundInfo>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<PlayerDisplay>();
			RootPanel.AddChild<ToastList>();
			RootPanel.AddChild<BallHistory>();
			RootPanel.AddChild<LoadingScreen>();
			RootPanel.AddChild<CursorController>();
		}
	}
}
