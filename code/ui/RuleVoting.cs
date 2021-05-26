
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System;
using System.Collections.Generic;

namespace PoolGame
{
	public class RuleCheckbox : Button
	{
		public bool IsEnabled { get; set; }
		public Rule Rule { get; private set; }

		public void SetRule( Rule rule )
		{
			Rule = rule;
		}

		public override void OnEvent( string eventName )
		{
			if ( Local.Pawn is not Player player || player.IsSpectator )
				return;

			if ( eventName == "onclick" )
			{
				if ( Rule.HasVote() )
				{
					Game.Instance.RemoveVote( Rule );
					IsEnabled = false;
				}
				else
				{
					Game.Instance.AddVote( Rule );
					IsEnabled = true;
				}
			}

			base.OnEvent( eventName );
		}

		public override void Tick()
		{
			SetClass( "enabled", IsEnabled );

			base.Tick();
		}
	}

	public class RuleVoteItem : Panel
	{
		public Label Name;
		public Label Description;
		public Rule Rule;
		public RuleCheckbox Checkbox;
		public Panel LeftSide;
		public Panel RightSide;
		public Image VoterOne;
		public Image VoterTwo;

		public RuleVoteItem()
		{
			LeftSide = Add.Panel( "left" );
			RightSide = Add.Panel( "right" );
			Name = LeftSide.Add.Label( "", "name" );
			Description = LeftSide.Add.Label( "", "description" );
			Checkbox = RightSide.AddChild<RuleCheckbox>( "checkbox" );
			VoterOne = RightSide.Add.Image( "", "voter" );
			VoterTwo = RightSide.Add.Image( "", "voter" );
		}

		public override void SetDataObject( object data )
		{
			if ( data is Rule rule )
			{
				Rule = rule;
				Name.Text = rule.Name;
				Description.Text = rule.Description;
				Checkbox.SetRule( rule );
			}
		}

		public override void Tick()
		{
			SetClass( "enabled", Rule.IsEnabled );

			var playerOne = Game.Instance.PlayerOne;
			var playerTwo = Game.Instance.PlayerTwo;

			if ( playerOne.IsValid() && playerTwo.IsValid() )
			{
				var clientOne = playerOne.GetClientOwner();
				var clientTwo = playerTwo.GetClientOwner();

				if ( Rule.HasVote( clientOne ) )
				{
					VoterOne.SetTexture( $"avatar:{clientOne.SteamId}" );
					VoterOne.SetClass( "hidden", false );
				}
				else
				{
					VoterOne.SetClass( "hidden", true );
				}

				if ( Rule.HasVote( clientTwo ) )
				{
					VoterTwo.SetTexture( $"avatar:{clientTwo.SteamId}" );
					VoterTwo.SetClass( "hidden", false );
				}
				else
				{
					VoterTwo.SetClass( "hidden", true );
				}
			}

			base.Tick();
		}
	}

	public class RuleVoting : Panel
	{
		public Panel Background;
		public Panel Container;
		public VirtualScrollPanel<RuleVoteItem> Rules;

		public RuleVoting()
		{
			StyleSheet.Load( "/ui/RuleVoting.scss" );

			Background = Add.Panel( "background" );
			Container = Add.Panel( "container" );

			Rules = Container.AddChild<VirtualScrollPanel<RuleVoteItem>>( "scroller" );

			foreach ( var rule in Game.Instance.Ruleset.Available )
				Rules.AddItem( rule );

			AcceptsFocus = true;
		}
	}
}
