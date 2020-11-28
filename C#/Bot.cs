using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using static TelegramBot.Config;

namespace TelegramBot
{
    internal static class EvalBot
    {
        private static ReplyKeyboardMarkup _keyboard = new ReplyKeyboardMarkup(new[]
        {
            new []
            {
                new KeyboardButton("Последнее значение")
            }
        });
        private static Dictionary<string, string> _users;

        private const string Path = "../../../users.json";

        private static readonly TelegramBotClient Bot =
            new TelegramBotClient(Token);

        private static double Eval(string expression)
        {
            var table = new System.Data.DataTable();
            return Convert.ToDouble(table.Compute(expression, string.Empty));
        }

        private static void ToJson()
        {
            using (var sw = new StreamWriter(Path))
            {
                var serializer = new JsonSerializer {Formatting = Formatting.Indented};
                serializer.Serialize(sw, _users);
            }
        }

        private static void FromJson()
        {
            if (File.Exists(Path))
            {
                using (var sr = File.OpenText(Path))
                {
                    var serializer = new JsonSerializer();
                    _users = (Dictionary<string, string>) serializer.Deserialize(sr,
                        typeof(Dictionary<string, string>));
                }
            }
            if (_users == null)
                _users = new Dictionary<string, string>();
        }

        private static void Main()
        {
            FromJson();
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message?.Type != MessageType.Text) return;
            if (message.Text == "Последнее значение")
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, _users[message.Chat.Id.ToString()]);
                return;
            }
            if (!Regex.IsMatch(message.Text, "[0-9()\\-+*%/.]"))
            {
                await Bot.SendTextMessageAsync(message.Chat.Id,
                    "Это не является допустимым для калькулятора выражением");
                return;
            }
            if (!_users.ContainsKey(message.Chat.Id.ToString()))
                _users.Add(message.Chat.Id.ToString(), "0");
            string result;
            try
            {
                result = Eval(message.Text).ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception)
            {
                await Bot.SendTextMessageAsync(message.Chat.Id,
                    "Это не является допустимым для калькулятора выражением");
                return;
            }
            await Bot.SendTextMessageAsync(message.Chat.Id, result, replyMarkup: _keyboard);
            _users[message.Chat.Id.ToString()] = result;
            ToJson();
        }
    }
}