using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;
using Telegram.Bot.Requests;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.InlineQueryResults;
using System.Collections.Generic;

namespace TelegramMusicBot
{
    class BotManager
    {
        private enum DialogState
        {
            Greetings,
            OptionsChoise,
            EnterLyrics,
        }

        private static DialogState currentState;

        const string token = "5695724139:AAF0Yf2HbpdEco6Y-k6C-1tFFEUD-T_GO7w";
        public static ITelegramBotClient client { get; set; } = new TelegramBotClient(token);

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
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Привет! Я бот. Могу помочь найти песню, а также много чего ещё.");

                        await botClient.SendTextMessageAsync(message.Chat.Id, "Что пожелаешь?", replyMarkup: GetReplyMarkup(
                           new List<string> { "Найти песню", "[ V O I D ]" }));

                        currentState = DialogState.OptionsChoise;
                        break;

                    default:
                        Console.WriteLine($"Unsupported message! {message.Text}");
                        break;
                }
            }
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
                        Console.WriteLine($"Unsupported message! {message.Text}");
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

                await botClient.SendTextMessageAsync(message.Chat.Id, "Что пожелаешь?", replyMarkup: GetReplyMarkup(
                            new List<string> { "Найти песню", "[ V O I D ]" }));

                currentState = DialogState.OptionsChoise;
            }
        }

        private async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;

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
        private static async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }
    }
}
