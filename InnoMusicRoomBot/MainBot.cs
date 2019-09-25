using InnoMusicRoomBot.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace InnoMusicRoomBot
{
    public class MainBot
    {
        public IConfiguration configuration;
        static ChatId adminChat = new ChatId(AppSettings.adminId);
        static ITelegramBotClient mainbot;
        public static List<Command> commands = new List<Command>();
        public MainBot()
        {
            if (AppSettings.isDevEnvironment)
            {
                ICredentials cread = new NetworkCredential(AppSettings.proxyLogin, AppSettings.proxyPassword);
                WebProxy proxy = new WebProxy(AppSettings.proxyAddress, false, null, cread);

                mainbot = new TelegramBotClient(AppSettings.mainKey, proxy);
            }
            else
            {
                mainbot = new TelegramBotClient(AppSettings.mainKey);
            }

            User me = mainbot.GetMeAsync().Result;
            Console.WriteLine(
                  $"Hello, World! I am user №{me.Id}, alias {me.Username} and my name is {me.FirstName}."
                );

            mainbot.OnMessage += Bot_OnMessage;
            mainbot.StartReceiving();

            //Этот процесс нужен для мониторинга жизнеспособности бота. Нет сообщений - бот отвалился.
            Thread countThread = new Thread(new ThreadStart(Count));
            countThread.Start();
        }
        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text == null)
            {
                return;
            }
            Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");
            //Запись в админского бота с логом

            AdminBot.adminLog(e.Message);

            switch (e.Message.Text)
            {
                case "/book":
                    BookCommand.ReplyWithSchedule(e.Message, mainbot);
                    break;
                case "/cancel":
                    CancelCommand.PerformAnswer(e.Message, mainbot);
                    break;
                case "/start":
                    StartCommand.PerformAnswer(e.Message, mainbot);
                    break;
                case "/help":
                    HelpCommand.PerformAnswer(e.Message, mainbot);
                    break;
                case "/checkin":
                    break;
                case "/status":
                    break;
                case "Пн":
                case "Вт":
                case "Ср":
                case "Чт":
                case "Пт":
                case "Сб":
                case "Вс":
                    BookCommand.ReplyWithTimeInput(e.Message, mainbot);
                    break;
            }


            //commands.Add(new HelloCommand());
            //Update[] updates = bot.GetUpdatesAsync().Result;

            //foreach( Update update in updates)
            //{
            //    foreach( Command command in commands)
            //    {
            //        command.Execute(update.Message, bot);
            //    }
            //}


            //Message x2 = await bot.SendTextMessageAsync(
            //  chatId: adminChat,
            //  text: $" still to admin: someone said:\n" + e.Message.Text,
            //  replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(
            //    "Check sendMessage method",
            //    "https://core.telegram.org/bots/api#sendmessage"
            //  ))
            //);
            //string testx = x.Text;
            //string testx3 = x.ToString();

        }

        public static void Count()
        {
            for (int i = 0; i < 10000; i++)
            {
                // int.MaxValue в микросекундах это 24,86 дня
                try
                {
                    Message mes = mainbot.SendTextMessageAsync(adminChat, $"check {i}").Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception message");
                    Console.WriteLine(e.Message);
                }
                Console.WriteLine($"Count check {i}");
                Thread.Sleep(600 * 1000);
            }
        }
    }
}
