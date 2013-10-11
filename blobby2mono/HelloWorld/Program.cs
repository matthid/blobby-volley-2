using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

    using System.Runtime.InteropServices;
namespace HelloWorld
{
    using BlobbyBotloader;

    class Program
    {
        class T : IFormattable, IFormatProvider
        {
            public string ToString(string format, IFormatProvider formatProvider)
            {
                return formatProvider.GetFormat(typeof(T)).ToString();
            }

            public object GetFormat(Type formatType)
            {
                return this;
            }
        }
        static void Main (string[] args)
		{
			Console.WriteLine ("Hello world {0}");

			Exception e = new Exception ("TEst");
			BlobbyBotloader.BotLoadInfo info = new BotLoadInfo ();
			info.file = Marshal.StringToHGlobalAnsi ("ImbaBot.dll");
			var bot = BotLoader.LoadBot (info);
			Console.WriteLine ("Loaded {0}", bot == null ? "{NULL}" : bot.ToString ());
			Thread.Sleep (10000);
			Marshal.FreeHGlobal (info.file);
			Console.Read ();
		}
    }
}
