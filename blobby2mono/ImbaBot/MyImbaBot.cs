using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImbaBot
{
    using System.IO;

    using BlobbyBotloader;

    public class MyImbaBot : IBot
    {
        private  BotManager manager;
		private bool isInit;
		
        public MyImbaBot()
        {
        }
		
		public void Init (BotManager manager)
		{
			this.manager = manager;
			manager.Debug ("Started bot with difficulty: {0} and starttime: {2}",
                manager.Difficulty, manager.StartTime);
			isInit = true;
		}
		
        public PlayerInput Step ()
		{
			if (!isInit)
				throw new InvalidOperationException ("Bot is not jet initialized");
			
			
            return new PlayerInput(false, true, false);
        }
    }

}
