using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hotel.Check_In.Management.Models
{
    public class Booking
    {
        public int CheckInId { get; set; }
        public int RoomNo { get; set; }
        public int CustomerId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string CheckInStatus { get; set; }
        public string PaymentStatus { get; set; }
        public int AdvancePaid { get; set; }
        public int TotalAmountPaid { get; set; }
        public int TotalAmountToBePaid { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string IdProof { get; set; }
    }
}