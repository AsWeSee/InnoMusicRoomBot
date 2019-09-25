using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InnoMusicRoomBot.Commands
{
    public class HelpCommand
    {
        public static void PerformAnswer(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            Message mes = client.SendTextMessageAsync(chatId, @"/book позволяет выбрать день бронирования. После этого нужно ввести время начала и время завершения. Тогда запись будет добавлена в календарь. Доступное время для бронирования: с 7 до 23 часов. Максимум в день доступно 2 часа, либо 5 часов для привелегированных
/cancel позволяет удалить одну из записей бронирования на этой неделе.", replyToMessageId: messageId).Result;
        }
    }
}
