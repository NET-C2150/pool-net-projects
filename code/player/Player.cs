using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

namespace PoolGame
{
	public partial class Player : BasePlayer
	{
		public bool IsFollowingWhiteBall { get; set; }

		public Player()
		{
			Controller = new PoolController();
			Camera = new PoolCamera();
		}

		private float _cuePullBack;

		public override void Spawn()
		{
			if ( IsServer )
			{
				var cue = new PoolCue
				{
					Owner = this
				};

				ActiveChild = cue;
			}

			base.Spawn();
		}

		public override void Respawn()
		{
			Game.Instance?.Round?.OnPlayerSpawn( this );

			base.Respawn();
		}

		protected override void Tick()
		{
			TickActiveChild();

			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( IsFollowingWhiteBall )
				return;

			var whiteBall = Game.Instance.WhiteBall;
			var playerCue = (ActiveChild as PoolCue);

			if ( whiteBall != null && playerCue != null )
			{
				if ( !Input.Down( InputButton.Attack1 ) )
				{
					var yaw = Input.Rot.Yaw();
					playerCue.WorldRot = Rotation.From( playerCue.WorldRot.Angles().WithYaw( yaw ) );
				}
				else
				{
					_cuePullBack = Math.Clamp( _cuePullBack + Input.MouseDelta.y, -10f, 60f );

					if ( _cuePullBack < -5f )
					{
						using ( Prediction.Off() )
						{
							// TODO: This is shit, work out another way to determine it.
							var force = Input.MouseDelta.y * 200f;
							whiteBall.PhysicsBody.Velocity = playerCue.WorldRot.Left * force;
							ResetAndFollowWhite();
						}
					}
				}

				playerCue.WorldPos = whiteBall.WorldPos - playerCue.WorldRot.Left * ( 250f + _cuePullBack );
			}
		}

		protected override void UseFail()
		{
			// Do nothing. By default this plays a sound that we don't want.
		}

		private void ResetAndFollowWhite()
		{
			ActiveChild.EnableDrawing = false;
			IsFollowingWhiteBall = true;
			_cuePullBack = 0f;
		}
	}
}
