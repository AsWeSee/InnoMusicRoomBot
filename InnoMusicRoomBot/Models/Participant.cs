using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnoMusicRoomBot.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public DateTime SelectedDate { get; set; }
        public string Status { get; set; }
        public List<Booking> Bookings { get; set; }

        public Participant(string name, string alias, DateTime selectedDate, string status)
        {
            Name = name;
            Alias = alias;
            SelectedDate = selectedDate;
            Status = status;
        }
    }
}
