using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoolGame
{
	[Library( "pool", Title = "Pool" )]
	partial class Game : Sandbox.Game
	{
		public TriggerWhiteArea WhiteArea { get; set; }
		public Player PreviousWinner { get; set; }
		public Player PreviousLoser { get; set; }
		public List<PoolBall> AllBalls { get; private set; }
		public Hud Hud { get; set; }

		public static Game Instance
		{
			get => Current as Game;
		}

		[Net] public BaseRound Round { get; private set; }
		[Net] public EntityHandle<PoolBall> WhiteBall { get; set; }
		[Net] public EntityHandle<PoolBall> BlackBall { get; set; }
		[Net] public EntityHandle<Player> CurrentPlayer { get; set; }
		[Net] public EntityHandle<Player> PlayerOne { get; set; }
		[Net] public EntityHandle<Player> PlayerTwo { get; set; }

		private BaseRound _lastRound;

		[ServerVar( "pool_min_players", Help = "The minimum players required to start." )]
		public static int MinPlayers { get; set; } = 2;

		public Game()
		{
			if ( IsServer )
			{
				Hud = new();
			}

			_ = StartTickTimer();
		}

		public void RespawnBlackBall()
		{
			var whiteBall = WhiteBall.Entity;
			var entities = All.Where( ( e ) => e is PoolBallSpawn );

			foreach ( var entity in entities )
			{
				if ( entity is PoolBallSpawn spawner )
				{
					if ( spawner.Type == PoolBallType.Black )
					{
						whiteBall.WorldPos = spawner.WorldPos;
						whiteBall.PhysicsBody.AngularVelocity = Vector3.Zero;
						whiteBall.PhysicsBody.Velocity = Vector3.Zero;
						whiteBall.PhysicsBody.ClearForces();

						return;
					}
				}
			}
		}

		public void RespawnWhiteBall()
		{
			var whiteBall = WhiteBall.Entity;
			var entities = All.Where( ( e ) => e is PoolBallSpawn );

			foreach ( var entity in entities )
			{
				if ( entity is PoolBallSpawn spawner )
				{
					if ( spawner.Type == PoolBallType.White )
					{
						whiteBall.WorldPos = spawner.WorldPos;
						whiteBall.PhysicsBody.AngularVelocity = Vector3.Zero;
						whiteBall.PhysicsBody.Velocity = Vector3.Zero;
						whiteBall.PhysicsBody.ClearForces();

						return;
					}
				}
			}
		}

		public void RemoveBall( PoolBall ball )
		{
			AllBalls.Remove( ball );
			ball.Delete();
		}

		public void RemoveAllBalls()
		{
			if ( AllBalls != null )
			{
				foreach ( var entity in AllBalls )
					entity.Delete();

				AllBalls.Clear();
			}
			else
				AllBalls = new();
		}

		public Player GetBallPlayer( PoolBall ball )
		{
			if ( PlayerOne.Entity.BallType == ball.Type )
				return PlayerOne;
			else if ( PlayerTwo.Entity.BallType == ball.Type )
				return PlayerTwo;
			else
				return null;
		}

		public Player GetOtherPlayer( Player player )
		{
			if ( player == PlayerOne )
				return PlayerTwo;
			else
				return PlayerOne;
		}

		public void RespawnAllBalls()
		{
			RemoveAllBalls();

			var entities = All.Where( ( e ) => e is PoolBallSpawn );

			Log.Warning( "Found " + entities.Count() + " Ball Spawners" );

			var spawners = new List<Entity>();
			spawners.AddRange( entities );

			foreach ( var entity in spawners )
			{
				if ( entity is PoolBallSpawn spawner )
				{
					Log.Info( "Spawning " + spawner.Type.ToString() + " Ball" );

					var ball = new PoolBall
					{
						RenderColor = spawner.RenderColor,
						WorldPos = spawner.WorldPos,
						Number = spawner.Number,
						Type = spawner.Type
					};

					if ( ball.Type == PoolBallType.White )
						WhiteBall = ball;
					else if ( ball.Type == PoolBallType.Black )
						BlackBall = ball;

					AllBalls.Add( ball );
				}
				else
				{
					Log.Warning( entity.EngineEntityName + " was not a spawner!" );
				}
			}
		}

		public void ChangeRound(BaseRound round)
		{
			Assert.NotNull( round );

			Round?.Finish();
			Round = round;
			Round?.Start();
		}

		public async Task StartSecondTimer()
		{
			while (true)
			{
				await Task.DelaySeconds( 1 );
				OnSecond();
			}
		}

		public async Task StartTickTimer()
		{
			while (true)
			{
				await Task.NextPhysicsFrame();
				OnTick();
			}
		}

		public override void DoPlayerNoclip( Sandbox.Player player )
		{
			// Do nothing. The player can't noclip in this mode.
		}

		public override void DoPlayerSuicide( Sandbox.Player player )
		{
			if ( player.LifeState == LifeState.Alive && Round?.CanPlayerSuicide == true )
			{
				// This simulates the player being killed.
				player.LifeState = LifeState.Dead;
				player.OnKilled();
				PlayerKilled( player );
			}
		}

		public override void PostLevelLoaded()
		{
			_ = StartSecondTimer();

			base.PostLevelLoaded();
		}

		public override void PlayerKilled( Sandbox.Player player )
		{
			Round?.OnPlayerKilled( player as Player );

			base.PlayerKilled( player );
		}

		public override void PlayerDisconnected( Sandbox.Player player, NetworkDisconnectionReason reason )
		{
			Log.Info( player.Name + " left, checking minimum player count..." );

			Round?.OnPlayerLeave( player as Player );

			base.PlayerDisconnected( player, reason );
		}

		public override Player CreatePlayer() => new();

		private void OnSecond()
		{
			CheckMinimumPlayers();
			Round?.OnSecond();
		}

		private void OnTick()
		{
			Round?.OnTick();

			if ( IsClient )
			{
				// We have to hack around this for now until we can detect changes in net variables.
				if ( _lastRound != Round )
				{
					_lastRound?.Finish();
					_lastRound = Round;
					_lastRound.Start();
				}

				if ( WhiteArea == null )
				{
					WhiteArea = All.OfType<TriggerWhiteArea>().FirstOrDefault();

					if ( WhiteArea != null )
						WhiteArea.MakeAreaQuad();
					else
						Log.Info( "null" );
				}
			}
		}

		private void CheckMinimumPlayers()
		{
			if ( Sandbox.Player.All.Count >= MinPlayers)
			{
				if ( Round is LobbyRound || Round == null )
				{
					ChangeRound( new PlayRound() );
				}
			}
			else if ( Round is not LobbyRound )
			{
				ChangeRound( new LobbyRound() );
			}
		}
	}
}
