
namespace BlobbyBotloader
{
	using System;
	using Mono.Simd;
	/// <summary>
	/// Gets the lua interface.
	/// Just here for convenience and maybe to make it easier to migrate existing bots.
	/// </summary>
	[Obsolete("This Api should only be used to migrate bots (not for new ones)")]
	public abstract class LuaInterface : IBot {
		
		private PlayerInput nextInput;
		
        private BotManager manager;
		private bool isInit;
		
		public double CONST_FIELD_WIDTH {
			get {
				return manager.CONST_FIELD_WIDTH;
			}
		}

		public double CONST_GROUND_HEIGHT {
			get {
				return manager.CONST_GROUND_HEIGHT;
			}
		}

		public double CONST_BALL_GRAVITY {
			get {
				return manager.CONST_BALL_GRAVITY;
			}
		}

		public double CONST_BALL_RADIUS {
			get {
				return manager.CONST_BALL_RADIUS;
			}
		}

		public double CONST_BLOBBY_JUMP {
			get {
				return manager.CONST_BLOBBY_JUMP;
			}
		}

		public double CONST_BLOBBY_BODY_RADIUS {
			get {
				return manager.CONST_BLOBBY_BODY_RADIUS;
			}
		}

		public double CONST_BLOBBY_HEAD_RADIUS {
			get {
				return manager.CONST_BLOBBY_HEAD_RADIUS;
			}
		}

		public double CONST_BLOBBY_HEIGHT {
			get {
				return manager.CONST_BLOBBY_HEIGHT;
			}
		}

		public double CONST_BLOBBY_GRAVITY {
			get {
				return manager.CONST_BLOBBY_GRAVITY;
			}
		}

		public double CONST_NET_HEIGHT {
			get {
				return manager.CONST_NET_HEIGHT;
			}
		}

		public  double CONST_NET_RADIUS {
			get {
				return manager.CONST_NET_RADIUS;
			}
		}
		
		const int WAITING_TIME = 1500;
		
		public LuaInterface ()
		{
		}
		
		void IBot.Init (BotManager manager)
		{
			this.manager = manager;
			manager.Debug ("Started bot with difficulty: {0} and starttime: {1}",
                manager.Difficulty, manager.StartTime);
			
			isInit = true;
			OnInit ();
		}
		
        PlayerInput IBot.Step ()
		{
			if (!isInit)
				throw new InvalidOperationException ("Bot is not jet initialized");
			
			nextInput.left = false;
			nextInput.right = false;
			nextInput.up = false;
			
			bool serving = false;
			if (manager.SelfServing) {
				serving = true;
				OnServe (!manager.BallDown);
			} else if (manager.OpponentServing) {
				OnOpponentServe ();	
			} else {
				if (manager.Bounce) {
					OnBounce ();
				}
				
				OnGame ();
			}
			
			if (manager.StartTime + WAITING_TIME > manager.Tick && serving) {
				return new PlayerInput (false, false, false);
			}
			
			return nextInput;
        }
		
		private PlayerInput CurrentInput {
			get {
				return nextInput;
			}
		}
		
		public int touches ()
		{
			return manager.SelfTouches;
		}
		
		public bool launched ()
		{
			return manager.SelfBlobJump;
		}
		
		public void debug (string data)
		{
			manager.Debug ("LUA-DEBUG: {0}", data);
		}
		
		public void jump ()
		{
			nextInput.up = true;
		}
		
		public void left ()
		{
			nextInput.left = true;
		}
		public void right ()
		{
			nextInput.right = true;
		}
		
		public void moveto (double targetX)
		{
			var currentX = manager.SelfBlobPosition.X;
			if (currentX > targetX + 2.0f) {
				left ();
			}
			if (currentX < targetX - 2.0f) {
				right ();	
			}
		}
		
		public double ballx ()
		{
			return manager.BallPosition.X;
		}
		
		
		public double bally ()
		{
			return manager.BallPosition.Y;
		}
		
		public double bspeedx ()
		{
			return manager.BallVelocity.X;	
		}
		
		public double bspeedy ()
		{
			return manager.BallVelocity.Y;	
		}
		public double posx ()
		{
			return manager.SelfBlobPosition.X;	
		}
		
		public double posy ()
		{
			return manager.SelfBlobPosition.Y;	
		}
		
		public double oppx ()
		{
			return manager.OpponentBlobPosition.X;	
		}
		
		public double oppy ()
		{
			return manager.OpponentBlobPosition.Y;	
		}
		
		[Obsolete("estimate is obsolete")]
		public double estimate ()
		{
			var pos = manager.BallPosition; 
			var vel = manager.BallVelocity;
			var time = (vel.Y - Math.Sqrt ((vel.Y * vel.Y) - (-2.0 * CONST_BALL_GRAVITY * (-pos.Y + CONST_GROUND_HEIGHT - CONST_BALL_RADIUS)))) / (-CONST_BALL_GRAVITY);
	
			var estim = pos.X + vel.X * time;
			return estim;
		}
		
		[Obsolete("estimx is obsolete")]
		public double estimx (int physicssteps)
		{
			return manager.BallPosition.X + physicssteps * manager.BallVelocity.X;
		}
		
		[Obsolete("estimy is obsolete")]
		public double estimy (int physicssteps)
		{
			return manager.BallPosition.Y + physicssteps * (manager.BallPosition.Y + 0.5 * CONST_BALL_GRAVITY * physicssteps);
		}
		
		public virtual void OnInit (){ }
		
		/// <summary>
		/// Wird aufgerufen nachdem der Ball abgepfiffen
		///	wurde und der gesteuerte Spieler nun
		///	aufschlägt. 
		/// </summary>
		/// <param name='ballready'>
		/// ballready gibt an, ob der Ball
		///	schon zurückgesetzt wurde.
		/// </param>
		public abstract void OnServe (bool ballready);
		
		/// <summary>
		/// Wird aufgerufen nachdem der Ball abgepfiffen
		/// und nun der Gegner aufschlägt.
		/// </summary>
		public abstract void OnOpponentServe();
		
		/// <summary>
		/// Wird während des regulären Spiels aufgerufen
		/// </summary>
		public abstract void OnGame();
		
		/// <summary>
		/// Wird aufgerufen, wenn sich die Richtung des Balls ändert
		/// </summary>
		public virtual void OnBounce ()
		{
		}
	}
}

