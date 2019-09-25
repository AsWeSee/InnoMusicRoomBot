using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnoMusicRoomBot.Commands
{
    public class StartCommand
    {
        public static void PerformAnswer(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            Message mes = client.SendTextMessageAsync(chatId, "Привет. Введи /book если ты хочешь забронировать комнату.", replyToMessageId: messageId).Result;
        }
    }
}
