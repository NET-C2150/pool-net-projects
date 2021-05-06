using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PoolGame
{
	[Library( "pool_ball" )]
	public partial class PoolBall : ModelEntity
	{
		public Player LastStriker { get; private set; }
		public PoolBallNumber Number { get; private set; }
		public PoolBallType Type { get; private set; }

		public void ResetLastStriker()
		{
			LastStriker = null;
		}

		public void StartPlacing()
		{
			EnableAllCollisions = false;
			PhysicsEnabled = false;
		}

		public void StopPlacing()
		{
			EnableAllCollisions = true;
			PhysicsEnabled = true;
		}

		public void SetType( PoolBallType type, PoolBallNumber number )
		{
			if ( type == PoolBallType.Black )
			{
				SetMaterialGroup( 8 );
			}
			else if ( type == PoolBallType.Red )
			{
				SetMaterialGroup( (int)number );
			}
			else if ( type == PoolBallType.Yellow )
			{
				SetMaterialGroup( (int)number + 8 );
			}

			Number = number;
			Type = type;
		}

		public void TryMoveTo( Vector3 worldPos, BBox within )
		{
			var worldOBB = CollisionBounds + worldPos;

			foreach (var ball in All.OfType<PoolBall>())
			{
				if ( ball != this )
				{
					var ballOBB = ball.CollisionBounds + ball.WorldPos;

					// We can't place on other balls.
					if ( ballOBB.Overlaps( worldOBB ) )
						return;
				}
			}

			if ( within.ContainsXY( worldOBB ) )
				WorldPos = worldPos.WithZ( WorldPos.z );
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/pool/pool_ball.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );

			PhysicsBody.GravityScale = 5f;

			Transmit = TransmitType.Always;
		}

		public virtual void OnEnterPocket( TriggerBallPocket pocket )
		{
			Log.Info( "Ball Entered Pocket On: " + (Host.IsClient ? "Client" : "Server") );
			Game.Instance.Round?.OnBallEnterPocket( this, pocket );
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			// Our last striker is the one responsible for this collision.
			if ( eventData.Entity is PoolBall other )
			{
				LastStriker = Game.Instance.CurrentPlayer;

				Game.Instance.Round?.OnBallHitOtherBall( this, other );
			}

			base.OnPhysicsCollision( eventData );
		}
	}
}
