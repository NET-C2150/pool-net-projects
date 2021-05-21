using System;
using Sandbox;

namespace PoolGame
{
	public static class EntityExtension
	{
		public static bool IsClientOwner( this Entity self, Client client )
		{
			return ( self.GetClientOwner() == client );
		}
	}
}
