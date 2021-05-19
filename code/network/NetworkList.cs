using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolGame
{
	public class NetworkList<T> : NetworkClass where T : unmanaged
	{
		public event Action OnListUpdated;

		public List<T> Values = new();

		public void Add( T item )
		{
			Values.Add( item );
			NetworkDirty( "Values", NetVarGroup.Net );
			OnListUpdated?.Invoke();
		}

		public void Remove( T item )
		{
			Values.Remove( item );
			NetworkDirty( "Values", NetVarGroup.Net );
			OnListUpdated?.Invoke();
		}

		public void Clear()
		{
			Values.Clear();
			NetworkDirty( "Values", NetVarGroup.Net );
			OnListUpdated?.Invoke();
		}


		public override bool NetWrite( NetWrite write )
		{
			base.NetWrite( write );

			write.Write( (short)Values.Count );

			foreach ( var v in Values )
			{
				write.Write( v );
			}

			return true;
		}

		public override bool NetRead( NetRead read )
		{
			base.NetRead( read );

			int count = read.Read<short>();

			Clear();

			for ( int i = 0; i < count; i++ )
			{
				Values.Add( read.Read<T>() );
			}

			OnListUpdated?.Invoke();

			return true;
		}
	}
}
