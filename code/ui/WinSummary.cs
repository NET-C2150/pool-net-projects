﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System;
using System.Collections.Generic;

namespace PoolGame
{
	public class RankIcon : Panel
	{
		public Panel Rank;
		public Label Level;

		public RankIcon()
		{
			Rank = Add.Panel( "rank" );
			Level = Add.Label( "0", "level" );
		}

		public void Update( PlayerRank rank, int level )
		{
			Rank.AddClass( rank.ToString().ToLower() );
			Level.Text = level.ToString();
		}
	}

	public class OpponentDisplay : Panel
	{
		public Label Text;
		public Panel Container;
		public Image Avatar;
		public Label Name;
		public RankIcon RankIcon;

		public OpponentDisplay()
		{
			Text = Add.Label( "", "text" );
			Container = Add.Panel( "background" );
			Avatar = Container.Add.Image( "", "avatar" );
			Name = Container.Add.Label( "", "name" );
			RankIcon = Container.AddChild<RankIcon>();
		}

		public void Update( EloOutcome outcome, Player opponent )
		{
			if ( outcome == EloOutcome.Win )
				Text.Text = "You beat";
			else
				Text.Text = "You lost to";

			var opponentClient = opponent.GetClientOwner();

			Avatar.SetTexture( $"avatar:{opponentClient.SteamId}" );
			Name.Text = opponentClient.Name;

			RankIcon.Update( opponent.Elo.GetRank(), opponent.Elo.GetLevel() );
		}
	}

	public class RankProgress : Panel
	{
		public RankIcon LeftRank;
		public RankIcon RightRank;
		public Panel BarBackground;
		public Panel BarProgress;
		public Panel BarDelta;

		public RankProgress()
		{
			LeftRank = AddChild<RankIcon>( "leftrank" );
			RightRank = AddChild<RankIcon>( "rightrank" );
			BarBackground = Add.Panel( "barbg" );
			BarProgress = Add.Panel( "barprogress" );
			BarDelta = Add.Panel( "bardelta" );
		}

		public void Update( EloScore score )
		{
			// I'm not a fan of doing it all this way... it'll do for the time being.
			var previousScore = new EloScore();
			previousScore.Rating = score.Rating - score.Delta;

			var nextScore = new EloScore();
			nextScore.Rating = previousScore.GetNextLevelRating();

			var progress = (nextScore.Rating - previousScore.Rating);
			var delta = Math.Min( score.Delta, 100 - progress );

			LeftRank.Update( previousScore.GetRank(), previousScore.GetLevel() );
			RightRank.Update( nextScore.GetRank(), nextScore.GetLevel() );

			BarProgress.Style.Width = Length.Percent( progress );
			BarDelta.Style.Width = Length.Percent( delta );

			Style.Dirty();
		}
	}

	public class WinSummary : Panel
	{
		public Panel Background;
		public Panel Container;
		public Panel Header;
		public RankProgress RankProgress;
		public OpponentDisplay OpponentDisplay;

		public WinSummary()
		{
			StyleSheet.Load( "/ui/WinSummary.scss" );

			Background = Add.Panel( "background" );
			Container = Add.Panel( "container" );
			Header = Container.Add.Panel( "header" );
			RankProgress = Container.AddChild<RankProgress>();
			OpponentDisplay = Container.AddChild<OpponentDisplay>();

			AcceptsFocus = true;
		}

		public void Update( EloOutcome outcome, Player opponent )
		{
			if ( Local.Pawn is Player player )
			{
				RankProgress.Update( player.Elo );
				OpponentDisplay.Update( outcome, opponent );

				if ( outcome == EloOutcome.Win )
					Header.AddClass( "win" );
				else
					Header.AddClass( "loss" );
			}
		}
	}
}
