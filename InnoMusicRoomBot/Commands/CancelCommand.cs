using InnoMusicRoomBot.Models;
using InnoMusicRoomBot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnoMusicRoomBot.Commands
{
    public class CancelCommand
    {
        public static void PerformAnswer(Message message, ITelegramBotClient client, Participant participant, bool currentWeek)
        {
            var chatId = message.Chat.Id;
            var messageId = message.MessageId;
            //Здесь нужно получить список броней на алиасе запросившего за эту неделю,
            //После чего вывести инлайном клавиатуру со списком броней
            //поймать колбэк запроса

            DateTime weekStart = BookCommand.weekStartDateForBooking(currentWeek);
            DateTime weekEnd = weekStart.AddDays(+7);

            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            using (MobileContext db = new MobileContext())
            {
                var bookingsWeek = db.Bookings.Where(c => (c.Participant == participant) && (c.TimeStart > weekStart) && (c.TimeEnd < weekEnd)).OrderBy(c => c.TimeStart);
                foreach (var booking in bookingsWeek)
                {
                    string caption = dayName(booking.TimeStart.DayOfWeek) + " " + booking.TimeStart.ToString("HH:mm") + " " + booking.TimeEnd.ToString("HH:mm");
                    buttons.Add(InlineKeyboardButton.WithCallbackData(caption, "cancel" + booking.Id));

                }
            }
            InlineKeyboardMarkup reply = new InlineKeyboardMarkup(buttons);
            Message mes = client.SendTextMessageAsync(chatId, "Выберите отменяемую бронь", replyMarkup: reply).Result;


            using (MobileContext db = new MobileContext())
            {
                participant.SelectedCurrentWeek = currentWeek;
                db.Participants.Update(participant);
                db.SaveChanges();
            }
        }
        public static string dayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday:
                    return "Пн";
                case DayOfWeek.Tuesday:
                    return "Вт";
                case DayOfWeek.Wednesday:
                    return "Ср";
                case DayOfWeek.Thursday:
                    return "Чт";
                case DayOfWeek.Friday:
                    return "Пт";
                case DayOfWeek.Saturday:
                    return "Сб";
                case DayOfWeek.Sunday:
                    return "Вс";
                default:
                    return "";
            }
        }

        public static void CallbackQuery(CallbackQueryEventArgs e, ITelegramBotClient bot)
        {
            string message = e.CallbackQuery.Data;
            var chatId = e.CallbackQuery.From.Id;

            int id = 0;
            int.TryParse(message.Substring(6), out id);

            AdminBot.adminLog(message);

            Participant participant;
            using (MobileContext db = new MobileContext())
            {
                try
                {
                    var booking = db.Bookings.Single(c => c.Id == id);
                    var bookingsWeek = db.Bookings.Remove(booking);
                    db.SaveChanges();
                } catch (InvalidOperationException ex)
                {
                    _ = bot.SendTextMessageAsync(chatId, "Бронь уже удалена").Result;
                    return;
                }

                participant = db.Participants.Single(c => c.Alias == e.CallbackQuery.From.Username);
            }

            using (FileStream fs = FormSchedule.FormScheduleImage(participant, participant.SelectedCurrentWeek))
            {
                Message mes = bot.SendPhotoAsync(chatId: chatId, photo: new InputOnlineFile(fs, "schedule.png"), caption: "Бронь отменена.").Result;
            }
        }
    }
}
