using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Dtos
{
    public class PatientPutDto
    {
        public string Patientname { get; set; } = null!;

        public int Sex { get; set; }

        public DateTime Birthday { get; set; }

        public double Height { get; set; }

        public double Weight { get; set; }

        public string? Deviceid { get; set; }
    }
}
