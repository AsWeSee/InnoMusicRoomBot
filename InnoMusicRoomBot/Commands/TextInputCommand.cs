using InnoMusicRoomBot.Models;
using InnoMusicRoomBot.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace InnoMusicRoomBot.Commands
{
    public class TextInputCommand
    {
        public static void Book(Message message, ITelegramBotClient bot, Participant participant)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;
            var selectedDay = participant.SelectedDate;


            //парсинг времени и запись брони в бд если всё ок.
            string[] splitted = message.Text.Split(" ");
            DateTime startTime;
            DateTime endTime;
            try
            {
                if (splitted[0].Length == 4)
                    splitted[0] = "0" + splitted[0];
                if (splitted[1].Length == 4)
                    splitted[1] = "0" + splitted[1];

                startTime = DateTime.ParseExact(splitted[0], "HH:mm", CultureInfo.InvariantCulture);
                endTime = DateTime.ParseExact(splitted[1], "HH:mm", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Message mes = bot.SendTextMessageAsync(message.Chat.Id, "Введите время начала и завершения в формате \"HH:MM HH:MM\". Например 12:20 14:20", replyToMessageId: message.MessageId).Result;
                AdminBot.adminLog("Exception " + ex.Message);
                return;
            }

            DateTime startDateTime = new DateTime(selectedDay.Year, selectedDay.Month, selectedDay.Day, startTime.Hour, startTime.Minute, 0);
            DateTime endDateTime = new DateTime(selectedDay.Year, selectedDay.Month, selectedDay.Day, endTime.Hour, endTime.Minute, 0);

            //на случай случайного ввода в неверном порядке
            if (startDateTime > endDateTime)
            {
                DateTime temp = startDateTime;
                startDateTime = endDateTime;
                endDateTime = temp;
            }

            if (startDateTime.Hour < 7)
            {
                Message mes = bot.SendTextMessageAsync(message.Chat.Id, "Время начала раньше 7 утра. (Время работы спорткомплекса: с 7:00 до 23:00)", replyToMessageId: message.MessageId).Result;
                AdminBot.adminLog("Время начала раньше 7 утра.");
                return;
            }
            if (startDateTime.Hour > 22 || (startDateTime.Hour == 22 && startDateTime.Minute >= 50))
            {
                Message mes = bot.SendTextMessageAsync(message.Chat.Id, "Время начала позже 22:50. (Время работы спорткомплекса: с 7:00 до 23:00)", replyToMessageId: message.MessageId).Result;
                AdminBot.adminLog("Время начала позже 22:50.");
                return;
            }
            if (endDateTime.Hour < 7)
            {
                Message mes = bot.SendTextMessageAsync(message.Chat.Id, "Время завершения раньше 7 утра. (Время работы спорткомплекса: с 7:00 до 23:00)", replyToMessageId: message.MessageId).Result;
                AdminBot.adminLog("Время завершения раньше 7 утра.");
                return;
            }
            if (endDateTime.Hour > 22 || (endDateTime.Hour == 22 && endDateTime.Minute > 50))
            {
                Message mes = bot.SendTextMessageAsync(message.Chat.Id, "Время завершения позже 22:50. (Время работы спорткомплекса: с 7:00 до 23:00)\n\nПо просьбе руководства спорткомплекса музкомната освобождается до закрытия ск.", replyToMessageId: message.MessageId).Result;
                AdminBot.adminLog("Время завершения позже 22:50.");
                return;
            }

            //Нужно получить доступное время для бронирования
            double freeDayTime = BookCommand.maxHoursToBookPerDay(participant.Status);
            using (MobileContext db = new MobileContext())
            {
                var bookings = db.Bookings.Where(c => (c.Participant == participant) && (c.TimeEnd.Year == selectedDay.Year) && (c.TimeEnd.Month == selectedDay.Month) && (c.TimeEnd.Day == selectedDay.Day)).ToList();
                foreach (var booking in bookings)
                {
                    freeDayTime -= (booking.TimeEnd.Subtract(booking.TimeStart).TotalHours);
                }

                if (startDateTime.Subtract(endDateTime).TotalHours >= freeDayTime)
                {
                    _ = bot.SendTextMessageAsync(chatId, $"Недостаточно доступного времени для бронирования.\n" +
                        $"Остаток на день {freeDayTime}\n" +
                        $"\n" +
                        $"Введите время начала и завершения брони. Например \"12:20 14:20\"", replyToMessageId: messageId).Result;
                    AdminBot.adminLog("Недостаточно доступного времени для бронирования.");
                    return;
                }

                double freeWeekTime = BookCommand.maxHoursToBookPerWeek(participant.Status);
                var bookingsWeek = FormSchedule.getBookingsForWeek(participant.SelectedCurrentWeek);
                foreach (var booking in bookingsWeek)
                {
                    freeWeekTime -= (booking.TimeEnd.Subtract(booking.TimeStart).TotalHours);
                }

                if (startDateTime.Subtract(endDateTime).TotalHours >= freeWeekTime)
                {
                    _ = bot.SendTextMessageAsync(chatId, $"Недостаточно доступного времени для бронирования.\n" +
                        $"Остаток на неделю {freeWeekTime}\n" +
                        $"\n" +
                        $"Введите время начала и завершения брони. Например \"12:20 14:20\"", replyToMessageId: messageId).Result;
                    AdminBot.adminLog("Недостаточно доступного времени для бронирования.");
                    return;
                }

                //TODO выяснить почему если убрать запрос к таблице Participants, то в таблице Bookings все ссылки на Participants будут null
                var parts = db.Participants.ToList();

                var allBookingsInDay = db.Bookings.Where(c => (c.TimeEnd.Year == startDateTime.Year) && (c.TimeEnd.Month == startDateTime.Month) && (c.TimeEnd.Day == startDateTime.Day)).ToList();
                foreach (var booking in allBookingsInDay)
                {
                    //В принципе, этот иф можно использовать внутри where при выборке броней
                    if ((startDateTime >= booking.TimeStart && startDateTime < booking.TimeEnd) ||
                        (endDateTime > booking.TimeStart && endDateTime <= booking.TimeEnd) ||
                        (booking.TimeStart > startDateTime && booking.TimeStart < endDateTime))
                    {
                        _ = bot.SendTextMessageAsync(message.Chat.Id, $"Выбранное время занято бронью @{booking.Participant.Alias}\n\nВведите время начала и завершения в формате \"HH:MM HH:MM\". Например 12:20 14:20", replyToMessageId: message.MessageId).Result;
                        AdminBot.adminLog($"Выбранное время занято бронью @{booking.Participant.Alias}");
                        return;
                    }
                }

                //Изза того что контекст бд закрывается, то ломается состояние participant. найти почему так либо не закрывать контекст
                participant = db.Participants.Single(c => c.Alias == message.From.Username);
                Booking newEntry = new Booking(participant, startDateTime, endDateTime);
                db.Bookings.Add(newEntry);
                db.SaveChanges();

                using (FileStream fs = FormSchedule.FormScheduleImage(participant, participant.SelectedCurrentWeek))
                {
                    Message mes = bot.SendPhotoAsync(chatId: chatId, photo: new InputOnlineFile(fs, "schedule.png"), caption: "Бронь установлена.", replyToMessageId: messageId).Result;
                }
            }
        }
    }
}
