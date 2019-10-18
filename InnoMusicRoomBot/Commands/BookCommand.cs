using InnoMusicRoomBot.Models;
using InnoMusicRoomBot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnoMusicRoomBot.Commands
{
    public class BookCommand
    {
        private static ReplyKeyboardMarkup reply = new ReplyKeyboardMarkup(new[] {
                    new KeyboardButton("Пн"),
                    new KeyboardButton("Вт"),
                    new KeyboardButton("Ср"),
                    new KeyboardButton("Чт"),
                    new KeyboardButton("Пт"),
                    new KeyboardButton("Сб"),
                    new KeyboardButton("Вс")}, true);
        public static void ReplyWithImageSchedule(Message message, ITelegramBotClient client, Participant participant)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            InputOnlineFile inputOnlineFile;
            using (FileStream fs = FormSchedule.FormScheduleImage(participant))
            {
                String caption = $"Ваш статус: {participant.Status}\nДоступно {maxHoursToBook(participant.Status)} часов бронирования в день.";
                inputOnlineFile = new InputOnlineFile(fs, "schedule.png");
                Message mes = client.SendPhotoAsync(chatId: chatId, photo: inputOnlineFile, caption: caption, replyToMessageId: messageId, replyMarkup: reply).Result;
            }
        }
        public static void ReplyWithTextSchedule(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            string text = FormSchedule.FormScheduleText();
            if (text.Equals("")) text = "В расписании пусто";
            Message mes = client.SendTextMessageAsync(chatId: chatId, text, replyToMessageId: messageId, replyMarkup: reply, parseMode: ParseMode.Markdown).Result;
        }

        public static void ReplyWithTimeInput(Message message, ITelegramBotClient client, Participant participant, int selectedDateNum)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;

            // Берём начало недели.
            DateTime now = DateTime.Today;
            int daynum = dayOfWeekInt(now);
            DateTime selectedDay = now.AddDays(-daynum + selectedDateNum);

            DateTime weekStart = now.AddDays(-daynum);
            DateTime weekEnd = weekStart.AddDays(+7);


            //Нужно получить доступное время для бронирования
            double freetime = maxHoursToBook(participant.Status);
            using (MobileContext db = new MobileContext())
            {
                //var bookingsWeek = db.Bookings.Where(c => (c.Participant == participant) && (c.TimeStart > weekStart) && (c.TimeEnd < weekEnd));

                //TODO: Вывыести списком все брони (хотя не факт что надо, расписание картинкой его успешно заменяет)
                //var allBookingsInSelectedDay = db.Bookings.Where(c => (c.TimeEnd.Year == selectedDay.Year) && (c.TimeEnd.Month == selectedDay.Month) && (c.TimeEnd.Day == selectedDay.Day)).ToList();


                var bookings = db.Bookings.Where(c => (c.Participant == participant) && (c.TimeEnd.Year == selectedDay.Year) && (c.TimeEnd.Month == selectedDay.Month) && (c.TimeEnd.Day == selectedDay.Day)).ToList();
                foreach (var booking in bookings)
                {
                    freetime -= (booking.TimeEnd.Subtract(booking.TimeStart).TotalHours);
                }

                participant.SelectedDate = selectedDay;
                db.Participants.Update(participant);
                db.SaveChanges();
            }
            Message mes = client.SendTextMessageAsync(chatId, $"Доступно часов для бронирования: {freetime}.\nВведите время начала и завершения брони. Например \"12:20 14:20\"", replyToMessageId: messageId).Result;
        }

        //Вынести эти функции в отдельные логичное место
        public static int dayOfWeekInt(DateTime date)
        {
            int result = (int)date.DayOfWeek;
            if (result == 0)
                result = 6;
            else
                result -= 1;

            return result;
        }
        public static int maxHoursToBook(string status)
        {
            switch (status)
            {
                case "Lord":
                    return 15;
                case "Senior":
                case "payer":
                    return 5;
                case "Freelance":
                case "free":
                    return 2;
            }
            //предусматривать исключение на случай получения некорректного статуса
            return 0;
        }
    }
}
