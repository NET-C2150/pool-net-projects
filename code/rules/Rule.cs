using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;

namespace PoolGame
{
	public partial class Rule : Entity
	{
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual bool EnabledByDefault => false;

		[Net] public List<ulong> Voters { get; set; }
		[Net] public bool IsEnabled { get; set; }

		public Rule()
		{
			if ( IsServer )
				IsEnabled = EnabledByDefault;

			Transmit = TransmitType.Always;
		}

		public bool HasVote()
		{
			Host.AssertClient();
			return Voters.Contains( Local.Client.SteamId );
		}

		public bool HasVote( Client client )
		{
			return Voters.Contains( client.SteamId );
		}

		public void AddVote( Client client )
		{
			Host.AssertServer();

			if ( !Voters.Contains( client.SteamId ) )
				Voters.Add( client.SteamId );

			UpdateEnabled();
		}

		public void RemoveVote( Client client )
		{
			Host.AssertServer();

			if ( Voters.Contains( client.SteamId ) )
				Voters.Remove( client.SteamId );

			UpdateEnabled();
		}

		private void UpdateEnabled()
		{
			IsEnabled = Voters.Count >= Game.MinVoterCount ? !EnabledByDefault : EnabledByDefault;
		}
	}
}
