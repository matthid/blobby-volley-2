
namespace UnionPort
{
	using System;
	using BlobbyBotloader;
	/// <summary>
	/// UNION
	/// 27.3.2007 - version 2
	/// Enormator
	/// 11.1.12 - insert game-provided physic constants where possible - by ngc92
	/// </summary>
	public class UnionBotPort : LuaInterface
	{
		bool OOSinit = false;
		bool OSinit = false;
		bool OGinit = false;
		int decision = 0;
		int funcs = 4;
		int oldtouches = 0;
		double oldbspeedx = 0;
		bool serve = false;
		double middle;
		double wallright;
		double blobbyheadheight;
		double blobbymaxjump;
		double blobbygroundpos;
		double netheight;
		double netcolissionleft;
		double netcolissionright;
		Random rand;
		public UnionBotPort ()
		{
		}
		
//Hauptfunktionen
//main functions

		public override void OnInit ()
		{
			//Startwerte setzen
			//set starting values
			OOSinit = false;
			OSinit = false;
			OGinit = false;
			decision = 0;
			funcs = 4;
			oldtouches = 0;
			oldbspeedx = 0;
			serve = false;
			rand = new Random ();
 			//Weltkonstanten definieren
			//define world constants
			middle = CONST_FIELD_WIDTH / 2;
			wallright = CONST_FIELD_WIDTH - CONST_BALL_RADIUS;
			blobbyheadheight = CONST_GROUND_HEIGHT + CONST_BLOBBY_HEIGHT + CONST_BALL_RADIUS;
			blobbymaxjump = 393.625;
			blobbygroundpos = CONST_GROUND_HEIGHT + CONST_BLOBBY_HEIGHT / 2;
			netheight = CONST_NET_HEIGHT + CONST_NET_RADIUS; // height is just the rod. for total height, we have to add the net-sphere-radius too
			netcolissionleft = middle - CONST_NET_RADIUS - CONST_BALL_RADIUS;
			netcolissionright = middle + CONST_NET_RADIUS + CONST_BALL_RADIUS;

		}

		public override void OnServe (bool ballready)
		{
			
			//initialisiere Funktion
			//initialise function
			OOSinit = false;
			OGinit = false;
			if (OSinit == false) { // einmal pro Aufruf von OnServe
				//one time per OnServe
				decision = decide (2); //Entscheidung faellen
				//make a decision
				OSinit = true;
				serve = true; //Aufschlag auch waehrend OnGame weitermachen
				//continue serve while OnGame
			}
			dofunc (2, decision, true); //Entscheidung ausfuehren
                            //follow the taken decision
		}

		public override void OnOpponentServe ()
		{
			//initialisiere Funktion
			//initialise function
			OSinit = false;
			OGinit = false;
			if (OOSinit == false) { //einmal pro Aufruf von OnOpponentServe
				//one time per OnOpponentServe
				decision = decide (1); //Entscheidung faellen
				//make a decision
				OOSinit = true;
				serve = false;
			}
			dofunc (1, decision, true); //Entscheidung ausfuehren
                            //follow the taken decision
		}

		public override void OnGame ()
		{ //initialisiere Funktion
			//initialise function
			OOSinit = false;
			OSinit = false;
			if ((OGinit == false) && (serve == false)) {  //einmal pro Aufruf von OnGame und wenn der Aufschlag schon vorbei ist
				//one time per OnGame and when serve is already over
				decision = decide (3); //Entscheidung faellen
				//make a decision
				OGinit = true;
			}
			if (serve == true) {
				//if still while serve
				dofunc (2, decision, true); //Der Funktion sagen, dass sie immernoch beim Aufschlag ist
				//tell the function, it's still OnServe
			} else {
				dofunc (3, decision, true); //Der Funktion sagen, dass sie im Spiel ist
				//tell the function that it's OnGame
			}
			if (ballx () > netcolissionleft + CONST_BALL_RADIUS) { //Falls Ball nicht mehr erreichbar
				//If Ball not gettable by Bot
				serve = false; //Aufschlagende erzwingen
				//Make an end to the serve
			}
			if ((touches () != oldtouches) || (Math.Abs (oldbspeedx) != Math.Abs (bspeedx ()))) { //Hat sich die Situation geändert
				//If the situation has changed
				OGinit = false; //gleich Entscheidung neu fällen
				//redo decision
				oldtouches = touches ();
				oldbspeedx = bspeedx ();
			}
		}

		public override void OnBounce ()
		{
		}
		
//Entscheidungsfunktionen
//decision functions
		
		public int decide (int funcno)
		{
			int t1 = 1;
			int chosen = 1;
			int chosenret = 0;
			//Alle Funktionen abfragen und die mit dem größten Return wählen
			//test all functions and take the one with the highest return value
			while (t1 <= funcs) {
				int temp = dofunc (funcno, t1, false);
				if (temp > chosenret) {
					chosen = t1;
					chosenret = temp;
				}
				t1 = t1 + 1;
			}
			debug (chosenret.ToString());//Sagt, für wie gut sich die gewählte Funktion hält
			//tells how good the chosen function says to fit
			return chosen;	
		}
		
		public int dofunc (int funcno, int actionno, bool action)
		{ //Weist jeder Aktionsnummer den echten Funktionsnamen zu
			//converts actionnumbers to the real names of the functions
			if (actionno == 1) {
				return std45deg (funcno, action);
			}
			if (actionno == 2) {
				return takelow (funcno, action);
			}
			if (actionno == 3) {
				return wait (funcno, action);
			}
			if (actionno == 4) {
				return twohitserve1 (funcno, action);
			}
			return 0;
		}
		
		
//Ausführbare Funktionen
//executable functions

		public int std45deg (int funcno, bool action)
		{ //spielt Ball aus der Luft bei maxjump im 45° Winkel an
			//plays ball in the air at height maxjump with 45° angle
			//funcno(s)=2,3
			double maxjump = blobbymaxjump;
			double distance = 32.25;
			double targetx = estimatex (maxjump) - distance;
			if (funcno == 1) {
				return -1;
			}
			if (action == false) {
				if ((funcno == 2) && (action == false)) {
					return rand.Next (10, 100);
				}
				if ((funcno == 3) && (action == false)) {
					if ((bspeedx () <= 3) && (Math.Max (balltimetox (targetx), balltimetoy (maxjump)) >= Math.Max (
						blobtimetoy (maxjump),
						blobtimetox (targetx)
					))) {
						int ret;
						if (bspeedx () == 0) {
							ret = 85;
						} else {
							ret = (int)Math.Min (Math.Pow(10.0, (-Math.Abs (bspeedx ())) + 1), 1) * 85;
						}
						if ((estimhitnet () == true) && (blobtimetox (netcolissionleft) <= balltimetoy (netheight))) { 
							ret = 190;
						}
						return ret;
					} else {
						return 0;
					}
				}
			}
			if (action == true) {
				if (funcno == 2) {
					if (posy () == 144.5) {
						moveto (targetx);
					}
					if ((Math.Abs (posx () - targetx) < 4.5) && (ballx () == 200) && (bally () == 299.5)) {
						jump ();
					}
				}
				if (funcno == 3) {
					moveto (targetx);
					if ((bally () < 580) && (bspeedy () < 0)) {
						jump ();
					}
				}
				//if (jumpcase) {
				if (false) { // No idea where this comes from!
					jump ();
				}
			}
			return 0;
		}

		public int takelow (int funcno, bool action)
		{ //Ballannahme ohne Sprung zum Stellen (bspeedx()=0) selten exakt
			//take ball without jump to create a ball with bspeedx()=0 (sellen exactly)
			//funcno(s)=3
			if (funcno == 1 || funcno == 2) {
				return -1;
			}
			if (action == false) {
				if (touches () <= 1) {
					return 1;
				} else {
					return -1;
				}
			}
			if (action == true) {
				moveto (estimatex (220.5));
			}
			return 0;
		}

		public int wait (int funcno, bool action)
		{ //Auf guter Position auf eine gegnerische Aktion warten
			//wait for an opponent's move at a good position
			//funcno(s)=1,3
			if (funcno == 2) {
				return -1;
			}
			if (funcno == 1 && action == false) {
				return 200;
			}
			if ((funcno == 3) && (action == false)) {
				if (estimatex (393.625) > 424.5) {
					return 200;
				} else {
					return -1;
				}
			}
			if (action == true) {
				moveto (180);
			}  
			return 0;
		}

		public int twohitserve1 (int funcno, bool action)
		{ //Aufschlag: Stellen (bspeedx()=0), rueber
			//serve: up (bspeedx()=0), play
			//funcno(s)=2
			if ((funcno == 1) || (funcno == 3)) {
				return -1;
			}
			if (action == false) {
				return rand.Next (10, 100);
			} else {
				if (touches () == 0) {
					moveto (200);
					if (Math.Abs (posx () - 200) < 5) {
						jump ();
					}
				}
				if (touches () == 1) {
					moveto (estimatex (blobbymaxjump) - 45);
					if ((bally () < 580) && (bspeedy () < 0)) {
						jump ();
					}
				}
				if (touches () == 3) {
					serve = false;
				}
			}
			return 0;
		}

//mathematische Hilfsfunktionen
//mathematical helpers

public double estimatex (double destY)
		{//gibt möglichst genaue Angabe der X-Koordinate zurück, bei der sich der Ball befindet, wenn er sich bei der angegebenen Y Koordinate befindet
			//returns exact ballx when bally will be given destY
			if ((bspeedy () == 0) && (bspeedx () == 0)) {
				return ballx ();
			}
			double time1 = (-bspeedy () - Math.Sqrt (Math.Pow (bspeedy (), 2) - (2 * CONST_BALL_GRAVITY * (bally () - destY)))) / CONST_BALL_GRAVITY;
			double resultX = (bspeedx () * time1) + ballx ();
			double estimbspeedx = bspeedx ();

			if (resultX > wallright) { // Korrigieren der Appraller an der Rechten Ebene
				resultX = 2 * CONST_FIELD_WIDTH - resultX;
				estimbspeedx = -estimbspeedx;
			}

			if (resultX < CONST_BALL_RADIUS) { // korrigieren der Appraller an der linken Ebene
				resultX = 2 * CONST_BALL_RADIUS - resultX;
				estimbspeedx = -estimbspeedx;
			}

			if ((resultX > netcolissionleft) && (estimatey (netcolissionleft - CONST_BALL_RADIUS) <= netheight) && (estimbspeedx > 0)) {
				resultX = 2 * netcolissionleft - resultX;
				estimbspeedx = -estimbspeedx;
			}
			return resultX;
		}

public double balltimetox (double x)
		{ //Zeit in Physikschritten, die der Ball bei der derzeitigen Geschwindigkeit zu der angegebenen X-Position braucht
			//time in physic steps, which the ball needs to reach the given x-position with momentany speed
			if (bspeedx () == 0) {
				return 10000;
			}
			double strecke = x - ballx ();
			double time = (strecke) / bspeedx ();
			return time;
	}

public double blobtimetoy (double y)
		{ //Zeit, die ein Blob braucht, um eine Y Position zu erreichen
			//time needed by a blob to reach a given y coordinate
			if (y > 383) {
				y = 383;
			}
			double grav = CONST_BLOBBY_GRAVITY / 2;    // half, because we use jump buffer
			double time1 = -CONST_BLOBBY_JUMP / grav + Math.Sqrt (2 * grav * (y - blobbygroundpos) + CONST_BLOBBY_JUMP * CONST_BLOBBY_JUMP) / grav;
			double time2 = -CONST_BLOBBY_JUMP / grav - Math.Sqrt (2 * grav * (y - blobbygroundpos) + CONST_BLOBBY_JUMP * CONST_BLOBBY_JUMP) / grav;
			double timemin = Math.Min (time1, time2);
			return timemin;
		}

public bool estimhitnet ()
		{ //Wird der Ball das Netz treffen (bool)
			//Will the ball hit the net (bool)
			int safety = 5;
			bool answer;
			return ((361.5 - safety < estimatex (323)) && (estimatex (323) < 438.5 + safety));
	}

double estimatey (double x) {
			//Y Position des Balls, wenn er sich an der angegebenen X Koordinate befindet
                       //y position of the ball, when it is at the given x coordinate
			double y=ballyaftertime(balltimetox(x));
			return y;
		}

double ballyaftertime (double t)
		{ //Y Position des Balls nach der angegebenen Zahl von Physikschritten
			//y position of the ball after the given time
			// NOTE:  Math.Pow(t, 2) maybe not right
			double y = 1 / 2 * CONST_BALL_GRAVITY * Math.Pow(t, 2) + bspeedy () * t + bally ();
			return y;
		}

double blobtimetox (double x) {//Zeit, die der Bot benoetigt, um eine gewisse X-Koordinate zu erreichen
                         //time needed for the bot to reach a given x-coordinate
			double time=Math.Abs(posx()-x)/4.5;
		return time;
	}

double balltimetoy (double y)
		{ //Zeit, die der Ball bis zu einer Y Position benoetigt
			//time needed by the ball to reach a given y position
			double time1 = -bspeedy () / CONST_BALL_GRAVITY + 1 / CONST_BALL_GRAVITY * Math.Sqrt (2 * CONST_BALL_GRAVITY * (y - bally ()) + Math.Pow (
				bspeedy (),
				2.0
			)
			);
			double time2 = -bspeedy () / CONST_BALL_GRAVITY - 1 / CONST_BALL_GRAVITY * Math.Sqrt (2 * CONST_BALL_GRAVITY * (y - bally ()) + Math.Pow (
				bspeedy (),
				2.0
			)
			);
			double timemax = Math.Max (time1, time2);
			return timemax;
		}

	}
}

