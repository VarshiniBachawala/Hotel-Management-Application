using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hotel.Check_In.Management.Models
{
    public class Rooms
    {
        public int RoomId { get; set; }
        public int RoomNo { get; set; }
        public String RoomType { get; set; }
        public int Price { get; set; }
    }
}