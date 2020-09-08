using InnoMusicRoomBot.Commands;
using InnoMusicRoomBot.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InnoMusicRoomBot.Tools
{
    public static class FormSchedule
    {
        public static FileStream FormScheduleImage(Participant participant, bool currentWeek)
        {
            //60, 91, левый верхний угол
            //279, 91
            //498, 91
            //279, 131
            //498, 171
            //219 на 40, размер секции длинной в час

            Image image = Image.FromFile("to_paint_form1280.png");
            //var form = File.OpenRead("schedule_form.png");

            Color lightGreen = Color.FromArgb(255, 123, 209, 72);
            Pen pen = new Pen(Color.Green, 3);
            SolidBrush lightGreenbrush = new SolidBrush(lightGreen);
            SolidBrush lightGraybrush = new SolidBrush(Color.LightGray);
            SolidBrush payerbrush = new SolidBrush(lightGreen);
            SolidBrush requesterBrush = new SolidBrush(Color.LightBlue);
            SolidBrush bookingBrush;
            SolidBrush redbrush = new SolidBrush(Color.Red); ;
            SolidBrush black = new SolidBrush(Color.Black);
            Font currentFont;
            Font fontSimple = new Font(FontFamily.GenericSansSerif, 10);
            Font fontBold = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);

            int xbase = 48;
            int ybase = 73;
            int xsize = 176;
            int ysize = 32;
            // Draw line to screen.
            using (var graphics = Graphics.FromImage(image))
            {
                int day = 0;
                var bookings = getBookingsForWeek(currentWeek);
                foreach (var booking in bookings)
                {
                    if (booking.Participant.Alias.Equals(participant.Alias))
                        currentFont = fontBold;
                    else
                        currentFont = fontSimple;

                    if (booking.Participant.Status.Equals("free"))
                        bookingBrush = lightGraybrush;
                    else
                        bookingBrush = payerbrush;


                    day = BookCommand.dayOfWeekInt(booking.TimeStart);
                    int ylength = (int)(ysize * (booking.TimeEnd.Subtract(booking.TimeStart).TotalMinutes / (float)60));
                    int xcorner = xbase + xsize * day;
                    int ycorner = ybase + (int)(ysize * ((booking.TimeStart.Hour - 7) + (booking.TimeStart.Minute / (float)60)));
                    graphics.FillRectangle(bookingBrush, xcorner, ycorner, xsize - 10, ylength - 5);


                    string caption = booking.Participant.Alias;
                    if (booking.TimeEnd.Subtract(booking.TimeStart).TotalHours > 1)
                        caption += "\n";
                    else
                        caption += " ";

                    graphics.DrawString(caption + booking.TimeStart.ToString("HH:mm") + " " + booking.TimeEnd.ToString("HH:mm"), currentFont, black, xcorner + 2, ycorner + 2);
                    _ = $"{booking.Participant.Alias} {booking.TimeStart} {booking.TimeEnd}\n";
                }

                int nowxcorner = xbase + xsize * BookCommand.dayOfWeekInt(DateTime.Today);
                int nowycorner = (int)(ybase + ysize * ((DateTime.Now.Hour - 7 + 3) + (DateTime.Now.Minute / (float)60))); // -7 сдвиг на 7 часов от старта календаря, +3 сдвиг на московское время
                graphics.FillRectangle(redbrush, nowxcorner, nowycorner, xsize, 2);

            }

            //TODO: Костыль. Переделать без сохранения в файл
            image.Save("result.png");
            return File.OpenRead("result.png");
        }
        public static string FormScheduleText()
        {
            string result = "";
            var bookings = getBookingsForWeek(true);
            foreach (var booking in bookings)
            {
                result += $"{booking.Participant.Alias} {booking.TimeStart} {booking.TimeEnd}\n";
            }
            return result;
        }

        private static List<Models.Booking> getBookingsForWeek(bool currentWeek)
        {
            DateTime weekStart = BookCommand.weekStartDateForBooking(currentWeek);
            DateTime weekEnd = weekStart.AddDays(+7);

            List<Models.Booking> bookings;
            using (MobileContext db = new MobileContext())
            {
                //TODO выяснить почему если убрать запрос к таблице Participants, то в таблице Bookings все ссылки на Participants будут null
                var parts = db.Participants.ToList();
                bookings = db.Bookings.Where(c => (c.TimeStart > weekStart) && (c.TimeEnd < weekEnd)).ToList();

            }
            return bookings;
        }
    }
}
