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
            string telegramToken = "your_token";
            string geniusToken = "your_token";

            var bot = new BotManager(telegramToken, geniusToken);

            bot.StartBot();
        }
       
    }
}
