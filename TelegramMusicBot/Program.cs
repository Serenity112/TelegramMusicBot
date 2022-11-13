using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramMusicBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new BotManager();

            bot.StartBot();
        }
       
    }
}
