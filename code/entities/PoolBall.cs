﻿using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "pool_ball" )]
	public partial class PoolBall : ModelEntity
	{
		public Player LastStriker { get; private set; }
		public PoolBallNumber Number { get; private set; }
		public PoolBallType Type { get; private set; }
		public bool IsAnimating { get; private set; }
		public TriggerBallPocket LastPocket { get; set; }

		public void ResetLastStriker()
		{
			LastStriker = null;
		}

		public void StartPlacing()
		{
			EnableAllCollisions = false;
			PhysicsEnabled = false;
		}

		public async Task AnimateIntoPocket()
		{
			PhysicsEnabled = false;
			IsAnimating = true;

			while ( true )
			{
				await Task.Delay( 30 );

				WorldScale = WorldScale.LerpTo( 0.69f /* nice */, Time.Delta * 4f );
				RenderAlpha = RenderAlpha.LerpTo( 0f, Time.Delta * 5f );

				if ( LastPocket != null && LastPocket.IsValid() )
					WorldPos = WorldPos.LerpTo( LastPocket.WorldPos + LastPocket.CollisionBounds.Center, Time.Delta * 16f );

				if ( RenderAlpha.AlmostEqual( 0f ) )
					break;
			}

			PhysicsEnabled = true;
			IsAnimating = false;
		}

		public void StopPlacing()
		{
			EnableAllCollisions = true;
			PhysicsEnabled = true;
			ResetInterpolation();
		}

		public void SetType( PoolBallType type, PoolBallNumber number )
		{
			if ( type == PoolBallType.Black )
			{
				SetMaterialGroup( 8 );
			}
			else if ( type == PoolBallType.Spots )
			{
				SetMaterialGroup( (int)number );
			}
			else if ( type == PoolBallType.Stripes )
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
			{
				WorldPos = worldPos.WithZ( WorldPos.z );
				ResetInterpolation();
			}
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/pool/pool_ball.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, true );

			Transmit = TransmitType.Always;
		}

		public virtual void OnEnterPocket( TriggerBallPocket pocket )
		{
			LastPocket = pocket;
			Game.Instance.Round?.OnBallEnterPocket( this, pocket );
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			// Our last striker is the one responsible for this collision.
			if ( eventData.Entity is PoolBall other )
			{
				LastStriker = Game.Instance.CurrentPlayer;
				Game.Instance.Round?.OnBallHitOtherBall( this, other );

				var sound = PlaySound( "ball-collide" );
				sound.SetPitch( Rand.Float( 0.9f, 1f ) );
				sound.SetVolume( (1f / 100f) * eventData.Speed );
			}
			else
			{
				// TODO: If this is the side entity (no way to determine it yet.)
				/*
				var sound = PlaySound( "ball-hit-side" );
				sound.SetPitch( Rand.Float( 0.8f, 1f ) );
				*/
			}

			Velocity = eventData.PostVelocity.WithZ( 0f );

			base.OnPhysicsCollision( eventData );
		}
	}
}
