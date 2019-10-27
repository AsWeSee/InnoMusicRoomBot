using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace InnoMusicRoomBot
{
    public class AdminBot
    {
        static ChatId adminChat = new ChatId(AppSettings.adminId);
        static ITelegramBotClient adminbot;
        public AdminBot()
        {
            //Т.к разработка ведётся в россии, то без прокси бот работать не будет.
            //if (AppSettings.isDevEnvironment)
            //{
                Console.WriteLine($"proxy enable");
                ICredentials cread = new NetworkCredential(AppSettings.proxyLogin, AppSettings.proxyPassword);
                WebProxy proxy = new WebProxy(AppSettings.proxyAddress, false, null, cread);

                adminbot = new TelegramBotClient(AppSettings.adminKey, proxy);
            //}
            //else
            //{
                //adminbot = new TelegramBotClient(AppSettings.adminKey);
            //}

            Console.WriteLine($"get me admin");
            User me2 = adminbot.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user №{me2.Id}, alias {me2.Username} and my name is {me2.FirstName}."
            );

            adminbot.OnMessage += AdminBot_OnMessage;
            adminbot.OnReceiveError += Bot_OnError;
            adminbot.StartReceiving();
        }

        public static async void adminLog(Message m)
        {
            if (adminbot != null)
            {
                try
                {
                    Message adminlog = await adminbot.SendTextMessageAsync(
                      chatId: adminChat,
                      text: $"@{m.From.Username}:\n" + m.Text
                    );
                } catch (Exception ex)
                {
                    Console.WriteLine("Error to log to admin bot");
                }
            }
        }
        void Bot_OnError(object sender, ReceiveErrorEventArgs e)
        {
            AdminBot.adminLog("error " + e.ApiRequestException.Message);
        }
        public static async void adminLog(String s)
        {
            if (adminbot != null)
            {
                Message adminlog = await adminbot.SendTextMessageAsync(
                  chatId: adminChat,
                  text: s
                );
            }
        }

        static async void AdminBot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"Received a text message in admin chat {e.Message.Chat.Id}.");
            Message x = await adminbot.SendTextMessageAsync(
              chatId: adminChat,
              text: $"Written to admin bot! @{e.Message.From.Username}:\n" + e.Message.Text
            );


            Message mes = adminbot.SendTextMessageAsync(e.Message.Chat.Id, "Вы не админ", replyToMessageId: e.Message.MessageId).Result;
            
        }
    }
}
