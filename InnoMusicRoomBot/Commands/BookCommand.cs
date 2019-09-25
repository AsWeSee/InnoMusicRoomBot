using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnoMusicRoomBot.Commands
{
    public class BookCommand
    {
        public static void ReplyWithSchedule(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;
            ReplyKeyboardMarkup reply = new ReplyKeyboardMarkup(
                new[] {
                    new KeyboardButton("Пн"),
                    new KeyboardButton("Вт"),
                    new KeyboardButton("Ср"),
                    new KeyboardButton("Чт"),
                    new KeyboardButton("Пт"),
                    new KeyboardButton("Сб"),
                    new KeyboardButton("Вс")
                }, true);
            Message mes = client.SendTextMessageAsync(chatId, "Image and book", replyToMessageId: messageId, replyMarkup: reply).Result;
        }

        public static void ReplyWithTimeInput(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;
            Message mes = client.SendTextMessageAsync(chatId, "Введите время начала и завершения в формате \"HH:MM HH:MM\"", replyToMessageId: messageId).Result;
        }
    }
}
