using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnoMusicRoomBot.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public Participant Participant { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public Booking()
        {
        }
        public Booking(Participant participant, DateTime timeStart, DateTime timeEnd)
        {
            Participant = participant;
            TimeStart = timeStart;
            TimeEnd = timeEnd;
        }
    }
}
