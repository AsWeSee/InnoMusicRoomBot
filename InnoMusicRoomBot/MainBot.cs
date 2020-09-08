using InnoMusicRoomBot.Commands;
using InnoMusicRoomBot.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                Console.WriteLine($"proxy enable");
                ICredentials cread = new NetworkCredential(AppSettings.proxyLogin, AppSettings.proxyPassword);
                WebProxy proxy = new WebProxy(AppSettings.proxyAddress, false, null, cread);

                mainbot = new TelegramBotClient(AppSettings.mainKey, proxy);
            }
            else
            {
                Console.WriteLine($"proxy disable");
                mainbot = new TelegramBotClient(AppSettings.mainKey);
            }
            Console.WriteLine($"get me main");
            User me = mainbot.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user №{me.Id}, alias {me.Username} and my name is {me.FirstName}.");

            mainbot.OnMessage += Bot_OnMessage; //TODO: Добавить сохранение отправленных картинок. прост по приколу
            mainbot.OnCallbackQuery += Bot_OnCallbackQuery;
            mainbot.OnReceiveError += Bot_OnError;
            mainbot.StartReceiving();

            //Этот процесс нужен для мониторинга жизнеспособности бота. Нет сообщений - бот отвалился.
            Thread countThread = new Thread(new ThreadStart(Count));
            countThread.Start();
        }

        private void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            CancelCommand.CallbackQuery(e, mainbot);
        }

        void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Message message = e.Message;
            if (message.Text == null)
                return;

            //Запись в админского бота с логом
            Console.WriteLine($"@{message.From.Username}: " + message.Text);
            AdminBot.adminLog(message);

            //проверка наличия человека в базе
            Participant participant;
            using (MobileContext db = new MobileContext())
            {
                var parts = db.Participants.ToList();
                //var books = db.Bookings.ToList();
                //db.Bookings.Add(new Booking(books[0].Participant, DateTime.Now, DateTime.Now));
                //db.SaveChanges();
                //db.Participants.Add(new Participant("Виталий", "RunGiantBoom", new DateTime(2019, 12, 25, 12, 40, 20), "owner"));
                //db.SaveChanges();
                try
                {
                    participant = db.Participants.Single(c => c.Alias == message.From.Username);
                }
                catch (Exception ex)
                {
                    mainbot.SendTextMessageAsync(message.Chat.Id, "Привет. Мы пока не знакомы. Напиши @RunGiantBoom чтобы получить доступ к музкомнате.", replyToMessageId: message.MessageId);
                    //Message mes = mainbot.SendTextMessageAsync(message.Chat.Id, "Привет. Мы пока не знакомы. Напиши @RunGiantBoom чтобы получить доступ к музкомнате.", replyToMessageId: message.MessageId).Result;
                    AdminBot.adminLog("Exception " + ex.Message);
                    return;
                }
            }

            //вызов команды при её наличии и выход из функции
            switch (message.Text)
            {
                case "/book":
                    BookCommand.ReplyWithImageSchedule(message, mainbot, participant, true);
                    return;
                case "/book_next_week":
                    if (participant.Status.Equals("Senior") || participant.Status.Equals("Lord"))
                    {
                        BookCommand.ReplyWithImageSchedule(message, mainbot, participant, false);
                    }
                    else
                    {
                        Message mes = mainbot.SendTextMessageAsync(message.Chat.Id, "Бронирование заранее за неделю доступно только патронов со статусом Senior. Чтобы поддержать нас, переходите на https://www.patreon.com/InnoMusicRoom.", replyToMessageId: message.MessageId).Result;
                        AdminBot.adminLog("book_next_week not allowed");
                    }
                    return;
                case "/book_text_version":
                    BookCommand.ReplyWithTextSchedule(message, mainbot);
                    return;
                case "/cancel":
                    CancelCommand.PerformAnswer(message, mainbot, participant, true);
                    return;
                case "/cancel_next_week":
                    CancelCommand.PerformAnswer(message, mainbot, participant, false);
                    return;
                case "/start":
                    StartCommand.PerformAnswer(message, mainbot);
                    return;
                case "/help":
                    HelpCommand.PerformAnswer(message, mainbot);
                    return;
                case "/checkin":
                    return;
                case "/status":
                    return;
                case "Пн":
                    BookCommand.ReplyWithTimeInput(message, mainbot, participant, 0);
                    return;
                case "Вт":
                    BookCommand.ReplyWithTimeInput(message, mainbot, participant, 1);
                    return;
                case "Ср":
                    BookCommand.ReplyWithTimeInput(message, mainbot, participant, 2);
                    return;
                case "Чт":
                    BookCommand.ReplyWithTimeInput(message, mainbot, participant, 3);
                    return;
                case "Пт":
                    BookCommand.ReplyWithTimeInput(message, mainbot, participant, 4);
                    return;
                case "Сб":
                    BookCommand.ReplyWithTimeInput(message, mainbot, participant, 5);
                    return;
                case "Вс":
                    BookCommand.ReplyWithTimeInput(message, mainbot, participant, 6);
                    return;
                default:
                    //Если команды в сообщении не было, значит пытаемся распарсить время и создать бронь
                    TextInputCommand.Book(message, mainbot, participant);
                    return;
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
        }
        void Bot_OnError(object sender, ReceiveErrorEventArgs e)
        {
            AdminBot.adminLog("error " + e.ApiRequestException.Message);
        }

        public void Count()
        {
            for (int i = 0; i < 10000; i++)
            {
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
                // int.MaxValue в микросекундах это 24,86 дня
                // 5 * 60 * 1000 это 5 минут
                // 60 * 60 * 1000 это 60 минут
                Thread.Sleep(60 * 60 * 1000);
            }
        }
    }
}
