using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnoMusicRoomBot.Commands
{
    public class BookCommand
    {
        public static void PerformAnswer(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            Message mes = client.SendTextMessageAsync(chatId, "Image and book", replyToMessageId: messageId).Result;
        }
    }
}
