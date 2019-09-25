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
            if (AppSettings.isDevEnvironment)
            {
                ICredentials cread = new NetworkCredential(AppSettings.proxyLogin, AppSettings.proxyPassword);
                WebProxy proxy = new WebProxy(AppSettings.proxyAddress, false, null, cread);

                adminbot = new TelegramBotClient(AppSettings.adminKey, proxy);
            }
            else
            {
                adminbot = new TelegramBotClient(AppSettings.adminKey);
            }

            User me2 = adminbot.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user №{me2.Id}, alias {me2.Username} and my name is {me2.FirstName}."
            );

            adminbot.OnMessage += AdminBot_OnMessage;
            adminbot.StartReceiving();
        }

        public static async void adminLog(Message m)
        {
            if (adminbot != null)
            {
                Message adminlog = await adminbot.SendTextMessageAsync(
                  chatId: adminChat,
                  text: $"@{m.From.Username}:\n" + m.Text
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
        }
    }
}
