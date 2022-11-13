using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using Genius;

namespace TelegramMusicBot
{
    class BotManager
    {
        public BotManager(string telegramToken, string geniusToken)
        {
            client = new TelegramBotClient(telegramToken);
            GeniusManager.client = new GeniusClient(geniusToken);
        }
        private enum DialogState
        {
            Greetings,
            OptionsChoise,
            EnterLyrics,
        }

        private static DialogState currentState;

        public static ITelegramBotClient client { get; set; }

        public void StartBot()
        {
            currentState = DialogState.Greetings;

            client.StartReceiving(HandleUpdateAsync, Error);

            Console.ReadLine();
        }

        private static IReplyMarkup GetReplyMarkup(List<string> options)
        {
            var reply = new List<List<KeyboardButton>>();

            foreach (var option in options)
            {
                reply.Add(new List<KeyboardButton> { option });
            }

            return new ReplyKeyboardMarkup(reply);
        }

        private async static Task ProcessGreetings(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            if (message.Text != null)
            {
                switch (message.Text)
                {
                    case "/start":
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Привет! Я бот. Могу помочь найти песню, а также много чего ещё.", replyMarkup: new ReplyKeyboardRemove());

                        await SendOptionChoise(botClient, update, cancellationToken);

                        currentState = DialogState.OptionsChoise;
                        break;

                    default:
                        ConsoleErrorOutput(message);
                        break;
                }
            }
        }

        private async static Task SendOptionChoise(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Что пожелаешь?", replyMarkup: GetReplyMarkup(
               new List<string> { "Найти песню", "[ V O I D ]" }));
        }

        private async static Task ProcessOptionChoise(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            if (message.Text != null)
            {
                switch (message.Text)
                {
                    case "Найти песню":
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введи песню, или какие-то строчки из неё..", replyMarkup: new ReplyKeyboardRemove());

                        currentState = DialogState.EnterLyrics;
                        break;

                    case "[ V O I D ]":
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введи песню, или какие-то строчки из неё..", replyMarkup: new ReplyKeyboardRemove());

                        currentState = DialogState.EnterLyrics;
                        break;

                    default:
                        ConsoleErrorOutput(message);
                        break;
                }
            }
        }

        private async static Task ProcessEnterLyrics(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            if (message.Text != null)
            {
                var hits = await GeniusManager.SearchHits(message.Text);

                string result = string.Empty;

                foreach (var item in hits)
                {
                    result += $"{item.Result.PrimaryArtist.Name} - {item.Result.Title}, [{item.Result.Stats.PageViews}]\n";
                }

                await botClient.SendTextMessageAsync(message.Chat.Id, result);

                await SendOptionChoise(botClient, update, cancellationToken);

                currentState = DialogState.OptionsChoise;
            }
        }

        private async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                switch (currentState)
                {
                    case DialogState.Greetings:
                        await ProcessGreetings(botClient, update, cancellationToken);
                        break;
                    case DialogState.OptionsChoise:
                        await ProcessOptionChoise(botClient, update, cancellationToken);
                        break;
                    case DialogState.EnterLyrics:
                        await ProcessEnterLyrics(botClient, update, cancellationToken);
                        break;
                }
            }        
        }

        private static void ConsoleErrorOutput(Message message)
        {
            Console.WriteLine($"Unsupported message: {message.Text} | Chat id: {message.Chat.Id} | Step: {currentState}");
        }

        private static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }
    }
}
