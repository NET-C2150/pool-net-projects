
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library]
	public partial class Hud : HudEntity<RootPanel>
	{
		public Panel Header { get; private set; }
		public Panel Footer { get; private set; }

		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<RoundInfo>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<ChatBox>();

			Header = RootPanel.Add.Panel( "header" );
			Header.AddChild<PlayerDisplay>();

			Footer = RootPanel.Add.Panel( "footer" );
			Footer.AddChild<ToastList>();
			Footer.AddChild<BallHistory>();

			RootPanel.AddChild<LoadingScreen>();
			RootPanel.AddChild<CursorController>();
		}
	}
}
