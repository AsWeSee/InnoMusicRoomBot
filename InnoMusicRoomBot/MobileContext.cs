using InnoMusicRoomBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnoMusicRoomBot
{
    public class MobileContext : DbContext
    {
        //public static readonly MobileContext INSTANCE = new MobileContext();
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Participant> Participants { get; set; }

        public MobileContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=musroom.db");
        }
    }
}
