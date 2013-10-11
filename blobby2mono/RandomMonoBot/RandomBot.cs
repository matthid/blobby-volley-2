using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomMonoBot
{
    using System.IO;

    using BlobbyBotloader;

    public class RandomBot : IBot
    {
        private Random rand;

        public RandomBot()
        {
            this.rand = new Random();
        }
		public void Init (BotManager manager)
		{
		}

        public PlayerInput Step ()
		{
			return new PlayerInput (rand.Next (10) > 5, rand.Next (10) > 5, rand.Next(100) > 95);
        }
    }
}
