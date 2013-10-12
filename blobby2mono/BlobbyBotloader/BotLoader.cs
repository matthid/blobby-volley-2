using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlobbyBotloader
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Security;
    using Mono.Simd;
	

    public enum PlayerSide : int
	{
		NO_PLAYER = -1,
		LEFT_PLAYER = 0,
		RIGHT_PLAYER = 1,
		MAX_PLAYERS,
	}
	
    public delegate PlayerInput DoStep();
	
    [StructLayout(LayoutKind.Sequential)]
	public struct MonoVector2 {
		public float x;
		public float y;
	}

    [StructLayout(LayoutKind.Sequential)]
	public struct WorldState
	{
		public sbyte ServingPlayer;
		
		public MonoVector2 BallPosition;
		public MonoVector2 BallVelocity;
		public MonoVector2 RightBlobPosition;
		public MonoVector2 LeftBlobPosition;
		
		public bool BallActive;
		public bool BallDown;
		public int RightScore;
		public int LeftScore;
		public bool RightBlobJump;
		public bool LeftBlobJump;
		
		public int RightTouches;
		public int LeftTouches;
		
		public uint Tick;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BotLoadInfo {
		public IntPtr file;
		public int difficulty;
		public uint startTime;
		public sbyte playerside;
		
		public float CONST_FIELD_WIDTH;
		public float CONST_GROUND_HEIGHT;
		public float CONST_BALL_GRAVITY;
		public float CONST_BALL_RADIUS;
		public float CONST_BLOBBY_JUMP;
		public float CONST_BLOBBY_BODY_RADIUS;
		public float CONST_BLOBBY_HEAD_RADIUS;
		public float CONST_BLOBBY_HEIGHT;
		public float CONST_BLOBBY_GRAVITY;
		public float CONST_NET_HEIGHT;
		public float CONST_NET_RADIUS;
	}
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerInput
    {
        public bool left;
        public bool right;
        public bool up;

        public PlayerInput(bool left, bool right, bool up)
        {
            this.left = left;
            this.right = right;
            this.up = up;
        }
    }
	
	
	public class BotManager {
		private string botName;
		private IBot bot;
		public int difficulty;
		public uint startTime;
		public PlayerSide playerside;
		
		public readonly double CONST_FIELD_WIDTH;
		public readonly double CONST_GROUND_HEIGHT;
		public readonly double CONST_BALL_GRAVITY;
		public readonly double CONST_BALL_RADIUS;
		public readonly double CONST_BLOBBY_JUMP;
		public readonly double CONST_BLOBBY_BODY_RADIUS;
		public readonly double CONST_BLOBBY_HEAD_RADIUS;
		public readonly double CONST_BLOBBY_HEIGHT;
		public readonly double CONST_BLOBBY_GRAVITY;
		public readonly double CONST_NET_HEIGHT;
		public readonly double CONST_NET_RADIUS;
		
		private readonly double HARDCODED_HEIGHT = 600.0;
		
		private PlayerSide servingPlayer;
		
		private Vector2d ballPosition;
		private Vector2d ballVelocity;
		private Vector2d opponentBlobPosition;
		private Vector2d selfBlobPosition;
		
		private bool ballActive;
		private bool ballDown;
		private int rightScore;
		private int leftScore;
		private bool rightBlobJump;
		private bool leftBlobJump;
		
		private int rightTouches;
		private int leftTouches;
		
		private uint tick;
		
		private bool bounce;
		private double lastBallSpeedX;
		
        internal PlayerInput StepBot (WorldState state)
		{
			ballActive = state.BallActive;
			ballDown = state.BallDown;
			
			var heightInvert = new Vector2d (0.0, HARDCODED_HEIGHT);
			
			// to get the x, 600.f-y value for positions
			ballPosition = heightInvert - InvertX (Convert (state.BallPosition));
			
			// to get the x, -y value for velocity
			ballVelocity = Vector2d.Zero - InvertX (Convert (state.BallVelocity));
			
			// to get the x, 600.f-y value
			opponentBlobPosition = heightInvert - InvertX (Convert (state.RightBlobPosition));
			
			// to get the x, 600.f-y value
			selfBlobPosition = heightInvert - InvertX (Convert (state.LeftBlobPosition));
			
			tick = state.Tick;
			
			// Change all values so bot has only be programmed for the left side
			if (playerside == PlayerSide.LEFT_PLAYER) {
				servingPlayer = (PlayerSide)state.ServingPlayer;
				rightScore = state.RightScore;
				leftScore = state.LeftScore;
				rightBlobJump = state.RightBlobJump;
				leftBlobJump = state.LeftBlobJump;
				rightTouches = state.RightTouches;
				leftTouches = state.LeftTouches;
			} else {
				servingPlayer = Invert ((PlayerSide)state.ServingPlayer);
				rightScore = state.LeftScore;
				leftScore = state.RightScore;
				rightBlobJump = state.LeftBlobJump;
				leftBlobJump = state.RightBlobJump;
				
				rightTouches = state.LeftTouches;
				leftTouches = state.RightTouches;
				
				var with = new Vector2d (CONST_FIELD_WIDTH, 0.0);
				// to set x to RIGHT_PLANE - x, for positions
				ballPosition = with + InvertX (ballPosition);
				// Invert x for velocity
				ballVelocity = InvertX (ballVelocity);
				var tempRightBlobPosition = opponentBlobPosition;
				opponentBlobPosition = with + InvertX (selfBlobPosition);
				selfBlobPosition = with + InvertX (tempRightBlobPosition);
			}
			
			ResetCache ();
			var step = bot.Step ();
			if (playerside == PlayerSide.LEFT_PLAYER) {
				return step;
			} else {
				return new PlayerInput (step.right, step.left, step.up);
			}
		}
		
		private void ResetCache ()
		{
			bounce = lastBallSpeedX != BallVelocity.X;
			lastBallSpeedX = BallVelocity.X;
		}
		
		private Vector2d Convert (MonoVector2 v)
		{
			return new Vector2d (v.x, v.y);
		}
		private Vector2d InvertX (Vector2d v)
		{
			return new Vector2d (- v.X, v.Y);
		}
		
		private PlayerSide Invert (PlayerSide side)
		{
			if (side == PlayerSide.LEFT_PLAYER)
				return PlayerSide.RIGHT_PLAYER;
			if (side == PlayerSide.RIGHT_PLAYER)
				return PlayerSide.LEFT_PLAYER;
			return side;
		}
		
		public BotManager (BotLoadInfo loadInfo)
		{
			botName = Marshal.PtrToStringAnsi (loadInfo.file);
			BotLoader.Debug ("Loading bot: " + (botName ?? "NULL"));
			bot = LoadBotObject (loadInfo);
			if (bot == null) {
				throw new NullReferenceException ("No Bot Found!");
			}
			difficulty = loadInfo.difficulty;
			startTime = loadInfo.startTime;
			playerside = (PlayerSide)loadInfo.playerside;
			
			CONST_FIELD_WIDTH = loadInfo.CONST_FIELD_WIDTH;
			CONST_GROUND_HEIGHT = loadInfo.CONST_GROUND_HEIGHT;
			CONST_BALL_GRAVITY = loadInfo.CONST_BALL_GRAVITY;
			CONST_BALL_RADIUS = loadInfo.CONST_BALL_RADIUS;
			CONST_BLOBBY_JUMP = loadInfo.CONST_BLOBBY_JUMP;
			CONST_BLOBBY_BODY_RADIUS = loadInfo.CONST_BLOBBY_BODY_RADIUS;
			CONST_BLOBBY_HEAD_RADIUS = loadInfo.CONST_BLOBBY_HEAD_RADIUS;
			CONST_BLOBBY_HEIGHT = loadInfo.CONST_BLOBBY_HEIGHT;
			CONST_BLOBBY_GRAVITY = loadInfo.CONST_BLOBBY_GRAVITY;
			CONST_NET_HEIGHT = loadInfo.CONST_NET_HEIGHT;
			CONST_NET_RADIUS = loadInfo.CONST_NET_RADIUS;
			bot.Init (this);
		}
		
		public bool SelfServing {
			get {
				// If no player is serving we assume the left (note playerside because of conversion)
				return 
					!BallActive && 
						(PlayerSide.LEFT_PLAYER == 
						 (ServingPlayer == PlayerSide.NO_PLAYER 
						 	? (playerside)
							: ServingPlayer));
			}
		}
		
		
		public bool OpponentServing {
			get {
				return 
					!BallActive && 
					(PlayerSide.LEFT_PLAYER != 
					(ServingPlayer == PlayerSide.NO_PLAYER 
						 	? (playerside)
							: ServingPlayer));
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether the ball has just bounced somewhere.
		/// </summary>
		/// <value>
		/// <c>true</c> if bounce; otherwise, <c>false</c>.
		/// </value>
		public bool Bounce {
			get {
				return bounce;
			}
		}
		
		/// <summary>
		/// Gets the playerside.
		/// This is only available for "hacking" purposes and should not be used.
		/// (If there is a bug in the conversion or the game for example).
		/// </summary>
		/// <value>
		/// The playerside in the real game.
		/// </value>
		/// <remarks>
		/// You have always code for the left side,
		/// the value of this property does not change this.
		/// </remarks>
		public PlayerSide Playerside {
			get {
				return this.playerside;
			}
		}

		public PlayerSide ServingPlayer {
			get {
				return servingPlayer;
			}
		}

		public Vector2d BallPosition {
			get {
				return this.ballPosition;
			}
		}

		public Vector2d BallVelocity {
			get {
				return this.ballVelocity;
			}
		}

		public Vector2d OpponentBlobPosition {
			get {
				return this.opponentBlobPosition;
			}
		}

		public Vector2d SelfBlobPosition {
			get {
				return this.selfBlobPosition;
			}
		}

		public bool BallActive {
			get {
				return this.ballActive;
			}
		}

		public bool BallDown {
			get {
				return this.ballDown;
			}
		}

		public int OpponentScore {
			get {
				return this.rightScore;
			}
		}

		public int SelfScore {
			get {
				return this.leftScore;
			}
		}

		public bool OpponentBlobJump {
			get {
				return this.rightBlobJump;
			}
		}

		public bool SelfBlobJump {
			get {
				return this.leftBlobJump;
			}
		}

		public uint Tick {
			get {
				return this.tick;
			}
		}
		
		public int Difficulty {
			get {
				return this.difficulty;
			}
			set {
				difficulty = value;
			}
		}
		
		public int OpponentTouches {
			get {
				return this.rightTouches;
			}
		}

		public int SelfTouches {
			get {
				return this.leftTouches;
			}
		}
		
		/// <summary>
		/// Gets the start time.
		/// </summary>
		/// <value>
		/// The start time.
		/// </value>
		public uint StartTime {
			get {
				return this.startTime;
			}
		}

		/// <summary>
		/// Gets the name of the bot. (the filename eg: bot.dll)
		/// </summary>
		/// <value>
		/// The name of the bot.
		/// </value>
		public string BotName {
			get {
				return this.botName;
			}
			private set {
				botName = value;
			}
		}
		/// <summary>
		/// Allows the bot to print a debug message.
		/// </summary>
		/// <param name='data'>
		/// Data of the message.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		public void Debug (string data, params object[] args)
		{
			BotLoader.Debug ("BOTDEBUG (" + BotName + "): " + data, args); 	
		}
		
		
        private static IBot LoadBotObject (BotLoadInfo loadInfo)
		{
			var botFile = Marshal.PtrToStringAnsi (loadInfo.file);
			var fullPath = Path.GetFullPath ("MonoBots");
			BotLoader.Debug ("Trying to load bot: {0}...", botFile);
			string log = "";
			log += string.Format ("\tLoading bots from: {0}\n", fullPath);
			foreach (var f in Directory.GetFiles(fullPath)) {
				log += string.Format ("\tCheck File: {0}\n", f);
				if (f.EndsWith (botFile)) {
					BotLoader.Debug ("File {0} looks good!!", f);
					var botAssembly = Assembly.LoadFile (Path.GetFullPath (f));
					BotLoader.Debug ("Loaded Assembly: {0}", botAssembly.GetName ().FullName);
					var bot = LoadFromAssembly (botAssembly, loadInfo);
					return bot;
				}
			}
			BotLoader.Debug ("Could not find bot! LOG:\n{0}", log);
			
            return null;
        }

	    private static IBot LoadFromAssembly (Assembly botAssembly, BotLoadInfo loadInfo)
		{
			var bottype = (from type in botAssembly.GetTypes ()
	                       where type.GetInterfaces ().Contains (typeof(IBot))
	                       select type).FirstOrDefault ();
			return (IBot)Activator.CreateInstance (bottype);
	    }
	}
	/// <summary>
	/// This interface is the API for the bot
	/// </summary>
	/// <remarks>
	/// This interface makes sure that the bot 
	/// has only be coded for the left side.
	/// </remarks>
	public interface IBot
	{
		/// <summary>
		/// Init the current Bot.
		/// This will be called on gamestart once.
		/// Make sure to save the manager to access the properties.
		/// </summary>
		/// <param name='manager'>
		/// This will give access to all the values needed by the bot.
		/// </param>
		void Init(BotManager manager);
		
		/// <summary>
		/// Ask the bot for the next step. 
		/// Will be called on every tick.
		/// </summary>
		PlayerInput Step();
	}

	public class BotLoader
	{
	    private static StreamWriter DebugFile;

	    private static bool init = false;
	    static BotLoader()
	    {
	        DoInit();
	    }

	    private static void DoInit ()
		{
			if (!init) {
				DebugFile = new StreamWriter ("monobot.log", true);
				Debug ("INIT LOADER");
				AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler (CurrentDomain_AssemblyResolve);
				AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler (CurrentDomain_TypeResolve);
				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler (CurrentDomain_UnhandledException);

				Debug ("STARTING BOTLOADER {0}", Assembly.GetExecutingAssembly ().GetName ().Version);
				
            }

	        init = true;
	    }

	    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug("Unhandled Exception... {0}", e.ExceptionObject);
        }

        static Assembly CurrentDomain_TypeResolve (object sender, ResolveEventArgs args)
		{
			Debug ("Trying to resolve type... {0}", args.Name);
			return null;
        }

        static Assembly CurrentDomain_AssemblyResolve (object sender, ResolveEventArgs args)
		{
			string log = "";
			try {
				Debug ("Trying to resolve {0} ...", args.Name);
			
				var libPath = Path.GetFullPath (Path.Combine ("mono", "lib", "mono", "4.5"));
			
				log += string.Format ("\tTrying to resolve type in {0}...\n", libPath);
			
				foreach (var f in Directory.GetFiles(libPath)) {
					var fullPath = Path.GetFullPath (f);
					string error = "";
					try {
						
						log += string.Format ("\tTrying file {0}...\n", fullPath);
						var name = AssemblyName.GetAssemblyName (fullPath);
						if (name.FullName == args.Name) {
							var assembly = Assembly.LoadFile (fullPath);
							Debug ("Successfull resolve with: {0}", fullPath);
							return assembly;
						}
					} catch (ArgumentException) {
						error = "assemblyFile {0} is invalid, such as an assembly with an invalid culture.";
					} catch (FileNotFoundException) {
						error = "assemblyFile {0} is not found.";
					} catch (SecurityException) {
						error = "The caller does not have path discovery permission ({0}).";
					} catch (BadImageFormatException) {
						error = "assemblyFile {0} is not a valid assembly. ";
					} catch (FileLoadException) {
						error = "An assembly or module ({0}) was loaded twice with two different sets of evidence";
					} 
					if (!string.IsNullOrEmpty (error)) {
						log += string.Format ("\t" + error+ "\n", fullPath);
					}
				}
			} catch (Exception ex) {
				Debug ("Error while trying to resolve.. {0}, \nERROR: {1}", args.Name, ex);
			}
			Debug ("Could not resolve...\n LOG: \n{0}", log);
            return null;
        }

        internal static void Debug(string data, params object[] args)
        {
            //DebugFile.WriteLine(string.Format(data, args));
            //DebugFile.Flush();
            Console.WriteLine(data, args);
        }

        public static BotManager LoadBot (BotLoadInfo loadInfo)
		{
			string file = "";
			try {
				return new BotManager (loadInfo);
			} catch (Exception e) {
				Debug ("Could not load Bot {0}(Error: {1})", file ?? "NULL", e == null ? "NULL" : e.ToString ());
            }

            return null;
        }

        public static PlayerInput StepBot (BotManager bot, WorldState state)
        { 
            try
            {
                if (bot == null)
                {
                    return new PlayerInput(false, false, false);
                }
                else
                {
                    return bot.StepBot(state);
                }
            }
            catch (Exception e)
            {
                Debug("Error: {0}", e);
            }

            return new PlayerInput(); // Do nothing
        }

        public static void UnloadBot (BotManager bot)
        {
            try
            {
            }
            catch (Exception e)
            {
                Debug("Error: {0}", e);
            }
        }

	}
}
