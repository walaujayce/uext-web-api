using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Dtos
{
    public class PatientGetDto
    {
        public string Patientname { get; set; } = null!;

        public string Patientid { get; set; } = null!;

        public int? Sex { get; set; }

        public DateTime Birthday { get; set; }

        public double Height { get; set; }

        public double Weight { get; set; }

        public string Bed { get; set; } = null!;

        public string Section { get; set; } = null!;

        public string Floor { get; set; } = null!;

        public string Deviceid { get; set; } = null!;

        public int Devicestatus { get; set; }
    }
}

