using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Dtos
{
    public class AlertPostDto
    {

        /// <summary>
        /// 警報控制開關
        /// </summary>
        public int Alertcontroller { get; set; }

        /// <summary>
        /// 警報開始時間，hour為單位
        /// </summary>
        public int Alertstarttime { get; set; }

        /// <summary>
        /// 警報停止時間，hour為單位
        /// </summary>
        public int Alertstoptime { get; set; }

        /// <summary>
        /// 警報間隔控制，以sec為單位
        /// </summary>
        public int Debounce { get; set; }

        /// <summary>
        /// 離床警報控制，當離床機率(%)大於閥值時發出警報, 靈敏度為3階: 高60%, 中70%, 低80%, 預設值為中70%
        /// </summary>
        public int Exitalert { get; set; }

        /// <summary>
        /// 姿態警報控制1，以sec為單位, 當姿態Posture為1, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec
        /// </summary>
        public int Posturealert1 { get; set; }

        /// <summary>
        /// 姿態警報控制2，以sec為單位, 當姿態Posture為, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec
        /// </summary>
        public int Posturealert2 { get; set; }

        /// <summary>
        /// 姿態警報控制3，以sec為單位, 當姿態Posture為3, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec
        /// </summary>
        public int Posturealert3 { get; set; }

        /// <summary>
        /// 姿態警報控制4，以sec為單位, 當姿態Posture為4, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec
        /// </summary>
        public int Posturealert4 { get; set; }

        /// <summary>
        /// 姿態警報控制5，以sec為單位, 當姿態Posture為5, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec
        /// </summary>
        public int Posturealert5 { get; set; }

        /// <summary>
        /// 姿態警報控制6，以sec為單位, 當姿態Posture為6, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec
        /// </summary>
        public int Posturealert6 { get; set; }

        /// <summary>
        /// 姿態警報控制7，以sec為單位, 當姿態Posture為7, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec
        /// </summary>
        public int Posturealert7 { get; set; }

        /// <summary>
        /// 脈搏上限控制，以Hz為單位, 當Heart Rate超出此設定(Hz)時發出警報, 預設值為100Hz
        /// </summary>
        public int Heartratehighlimit { get; set; }

        /// <summary>
        /// 脈搏下限控制，以Hz為單位, 當Heart Rate超出此設定(Hz)時發出警報, 預設值為100Hz
        /// </summary>
        public int Heartratelowlimit { get; set; }

        /// <summary>
        /// 呼吸上限控制
        /// </summary>
        public int Respiratoryratehighlimit { get; set; }

        /// <summary>
        /// 呼吸下限控制
        /// </summary>
        public int Respiratoryratelowlimit { get; set; }

        public bool Enablealert1 { get; set; }

        public bool Enablealert2 { get; set; }

        public bool Enablealert3 { get; set; }

        public bool Enablealert4 { get; set; }

        public bool Enablealert5 { get; set; }

        public bool Enablealert6 { get; set; }

        public bool Enablealert7 { get; set; }

        public string Patientid { get; set; } = null!;

    }
}
