using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soluvion.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public User Customer { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public int? EmployeeId { get; set; }
        public User Employee { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int StatusId { get; set; }
        public AppointmentStatus AppointmentStatus { get; set; } // Renamed to avoid ambiguity
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Kényelmi tulajdonság a státusz színének meghatározásához
        public Color StatusColor
        {
            get
            {
                if (AppointmentStatus == null) return Colors.Gray;

                return AppointmentStatus.StatusName switch
                {
                    "Pending" => Colors.Orange,
                    "Confirmed" => Colors.Green,
                    "Completed" => Colors.Blue,
                    "Cancelled" => Colors.Red,
                    "NoShow" => Colors.Purple,
                    _ => Colors.Gray
                };
            }
        }
    }
}
