using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UMonitorWebAPI.Utility;


namespace UMonitorWebAPI.Models;

public partial class UneoWebContext : DbContext
{
    public UneoWebContext()
    {
    }

    public UneoWebContext(DbContextOptions<UneoWebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Errorlog> Errorlogs { get; set; }

    public virtual DbSet<Floor> Floors { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Rawdatum> Rawdata { get; set; }

    public virtual DbSet<Recorddatum> Recorddata { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    var config = new ConfigurationBuilder()
    //        .SetBasePath(AppContext.BaseDirectory)
    //        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    //        .AddEnvironmentVariables() // 加入環境變數
    //        .Build();

    //    // 從環境變數取得值
    //    var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
    //    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "123";

    //    // 動態建立連線字串
    //    var connectionString = $"Host={dbServer};Port=5432;Database=uneo_web;Username=postgres;Password={dbPassword};";
    //    Console.WriteLine("OnConfiguring：" +　connectionString);

    //    optionsBuilder.UseNpgsql(connectionString);
    //}

    private string ip = "";
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

#if DEBUG
        //ip = Utility.Utility.GetLocalIPv4();
        ip = "192.9.120.142";
#else

        if (Config.IsAmber)
        {
            //Amber Docker IP
            ip = Utility.Utility.GetDockerContainerBridgeGatewayAddress();
        }
        else if (Config.IsIIS)
        {
            //Windows IIS IP
            ip = Utility.Utility.GetServerIPAddress();
        }
        else 
        {
            ip = "192.9.120.29";
        }
#endif
        optionsBuilder.UseNpgsql($"Host={ip};Port=5432;Database=uneo_web;Username=postgres;Password=uccc07568009");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Alertguid).HasName("alerts_pk");

            entity.ToTable("alerts");

            entity.Property(e => e.Alertguid)
                .ValueGeneratedNever()
                .HasColumnName("alertguid");
            entity.Property(e => e.Alertcontroller)
                .HasDefaultValue(3)
                .HasComment("警報控制開關")
                .HasColumnName("alertcontroller");
            entity.Property(e => e.Alertstarttime)
                .HasDefaultValue(0)
                .HasComment("警報開始時間，hour為單位")
                .HasColumnName("alertstarttime");
            entity.Property(e => e.Alertstoptime)
                .HasDefaultValue(24)
                .HasComment("警報停止時間，hour為單位")
                .HasColumnName("alertstoptime");
            entity.Property(e => e.Debounce)
                .HasDefaultValue(60)
                .HasComment("警報間隔控制，以sec為單位")
                .HasColumnName("debounce");
            entity.Property(e => e.Enablealert1)
                .HasDefaultValue(false)
                .HasColumnName("enablealert1");
            entity.Property(e => e.Enablealert2)
                .HasDefaultValue(false)
                .HasColumnName("enablealert2");
            entity.Property(e => e.Enablealert3)
                .HasDefaultValue(false)
                .HasColumnName("enablealert3");
            entity.Property(e => e.Enablealert4)
                .HasDefaultValue(false)
                .HasColumnName("enablealert4");
            entity.Property(e => e.Enablealert5)
                .HasDefaultValue(false)
                .HasColumnName("enablealert5");
            entity.Property(e => e.Enablealert6)
                .HasDefaultValue(false)
                .HasColumnName("enablealert6");
            entity.Property(e => e.Enablealert7)
                .HasDefaultValue(false)
                .HasColumnName("enablealert7");
            entity.Property(e => e.Exitalert)
                .HasDefaultValue(70)
                .HasComment("離床警報控制，當離床機率(%)大於閥值時發出警報, 靈敏度為3階: 高60%, 中70%, 低80%, 預設值為中70%")
                .HasColumnName("exitalert");
            entity.Property(e => e.Heartratehighlimit)
                .HasDefaultValue(100)
                .HasComment("脈搏上限控制，以Hz為單位, 當Heart Rate超出此設定(Hz)時發出警報, 預設值為100Hz")
                .HasColumnName("heartratehighlimit");
            entity.Property(e => e.Heartratelowlimit)
                .HasDefaultValue(60)
                .HasComment("脈搏下限控制，以Hz為單位, 當Heart Rate超出此設定(Hz)時發出警報, 預設值為100Hz")
                .HasColumnName("heartratelowlimit");
            entity.Property(e => e.Patientid)
                .HasMaxLength(16)
                .HasColumnName("patientid");
            entity.Property(e => e.Posturealert1)
                .HasDefaultValue(600)
                .HasComment("姿態警報控制1，以sec為單位, 當姿態Posture為1, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec")
                .HasColumnName("posturealert1");
            entity.Property(e => e.Posturealert2)
                .HasDefaultValue(600)
                .HasComment("姿態警報控制2，以sec為單位, 當姿態Posture為, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec")
                .HasColumnName("posturealert2");
            entity.Property(e => e.Posturealert3)
                .HasDefaultValue(600)
                .HasComment("姿態警報控制3，以sec為單位, 當姿態Posture為3, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec")
                .HasColumnName("posturealert3");
            entity.Property(e => e.Posturealert4)
                .HasDefaultValue(600)
                .HasComment("姿態警報控制4，以sec為單位, 當姿態Posture為4, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec")
                .HasColumnName("posturealert4");
            entity.Property(e => e.Posturealert5)
                .HasDefaultValue(600)
                .HasComment("姿態警報控制5，以sec為單位, 當姿態Posture為5, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec")
                .HasColumnName("posturealert5");
            entity.Property(e => e.Posturealert6)
                .HasDefaultValue(600)
                .HasComment("姿態警報控制6，以sec為單位, 當姿態Posture為6, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec")
                .HasColumnName("posturealert6");
            entity.Property(e => e.Posturealert7)
                .HasDefaultValue(600)
                .HasComment("姿態警報控制7，以sec為單位, 當姿態Posture為7, 而且狀態維持時間Duration超出此設定時間(sec)時發出警報, 預設值為600sec")
                .HasColumnName("posturealert7");
            entity.Property(e => e.Respiratoryratehighlimit)
                .HasDefaultValue(20)
                .HasComment("呼吸上限控制")
                .HasColumnName("respiratoryratehighlimit");
            entity.Property(e => e.Respiratoryratelowlimit)
                .HasDefaultValue(12)
                .HasComment("呼吸下限控制")
                .HasColumnName("respiratoryratelowlimit");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Deviceid).HasName("devices_pk");

            entity.ToTable("devices");

            entity.Property(e => e.Deviceid)
                .HasMaxLength(16)
                .HasColumnName("deviceid");
            entity.Property(e => e.Actionid)
                .HasMaxLength(6)
                .HasDefaultValueSql("'99'::character varying")
                .HasColumnName("actionid");
            entity.Property(e => e.Bed)
                .HasMaxLength(8)
                .HasColumnName("bed");
            entity.Property(e => e.BoxYStart)
                .HasDefaultValue(7)
                .HasColumnName("box_y_start");
            entity.Property(e => e.Connect)
                .HasDefaultValue(false)
                .HasColumnName("connect");
            entity.Property(e => e.DebFps)
                .HasDefaultValue(1)
                .HasColumnName("deb_fps");
            entity.Property(e => e.DebTst)
                .HasDefaultValue(1)
                .HasColumnName("deb_tst");
            entity.Property(e => e.Devicestatus)
                .HasDefaultValue(0)
                .HasColumnName("devicestatus");
            entity.Property(e => e.Devicetype).HasColumnName("devicetype");
            entity.Property(e => e.DisconnectCnt)
                .HasDefaultValue(0)
                .HasColumnName("disconnect_cnt");
            entity.Property(e => e.EdgeSitPoint)
                .HasDefaultValue(3)
                .HasColumnName("edge_sit_point");
            entity.Property(e => e.Edgebox)
                .HasDefaultValue(40)
                .HasColumnName("edgebox");
            entity.Property(e => e.Edgepar)
                .HasDefaultValue(90)
                .HasColumnName("edgepar");
            entity.Property(e => e.Emasize)
                .HasDefaultValue(1)
                .HasColumnName("emasize");
            entity.Property(e => e.Emathres)
                .HasDefaultValue(10)
                .HasColumnName("emathres");
            entity.Property(e => e.ErMap)
                .HasColumnType("character varying")
                .HasColumnName("er_map");
            entity.Property(e => e.Floor)
                .HasMaxLength(8)
                .HasColumnName("floor");
            entity.Property(e => e.HeightTh)
                .HasDefaultValue(12)
                .HasColumnName("height_th");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(16)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Judgemethod)
                .HasDefaultValue(1)
                .HasColumnName("judgemethod");
            entity.Property(e => e.Macaddress)
                .HasMaxLength(16)
                .HasColumnName("macaddress");
            entity.Property(e => e.Noisethres)
                .HasDefaultValue(2)
                .HasColumnName("noisethres");
            entity.Property(e => e.Pmio)
                .HasDefaultValue(100)
                .HasColumnName("pmio");
            entity.Property(e => e.RecordMode)
                .HasMaxLength(2)
                .HasDefaultValueSql("'02'::character varying")
                .HasColumnName("record_mode");
            entity.Property(e => e.Section)
                .HasMaxLength(8)
                .HasColumnName("section");
            entity.Property(e => e.Sitbox)
                .HasDefaultValue(40)
                .HasColumnName("sitbox");
            entity.Property(e => e.Sitpar)
                .HasDefaultValue(90)
                .HasColumnName("sitpar");
            entity.Property(e => e.Updatedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Used)
                .HasDefaultValue(true)
                .HasColumnName("used");
            entity.Property(e => e.Version)
                .HasMaxLength(20)
                .HasColumnName("version");
            entity.Property(e => e.Vmax)
                .HasDefaultValue(800)
                .HasColumnName("vmax");
            entity.Property(e => e.Vmin)
                .HasDefaultValue(200)
                .HasColumnName("vmin");
        });

        modelBuilder.Entity<Errorlog>(entity =>
        {
            entity.HasKey(e => e.Guid).HasName("errorlog_pk");

            entity.ToTable("errorlog");

            entity.Property(e => e.Guid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("guid");
            entity.Property(e => e.CheckStatus)
                .HasDefaultValue(false)
                .HasColumnName("check_status");
            entity.Property(e => e.Deviceid)
                .HasMaxLength(16)
                .HasColumnName("deviceid");
            entity.Property(e => e.Log)
                .HasMaxLength(225)
                .HasColumnName("log");
            entity.Property(e => e.Logtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("logtime");
            entity.Property(e => e.Logtype)
                .HasMaxLength(10)
                .HasColumnName("logtype");
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.Floorid).HasName("floor_pkey");

            entity.ToTable("floor");

            entity.Property(e => e.Floorid)
                .ValueGeneratedNever()
                .HasColumnName("floorid");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CheckStatus)
                .HasDefaultValue(false)
                .HasColumnName("check_status");
            entity.Property(e => e.CreateDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
            entity.Property(e => e.Deviceid)
                .HasMaxLength(20)
                .HasColumnName("deviceid");
            entity.Property(e => e.NotifyBody)
                .HasMaxLength(255)
                .HasColumnName("notify_body");
            entity.Property(e => e.PunchTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("punch_time");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Patientid).HasName("patients_pk");

            entity.ToTable("patients");

            entity.Property(e => e.Patientid)
                .HasMaxLength(16)
                .HasColumnName("patientid");
            entity.Property(e => e.Birthday)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("birthday");
            entity.Property(e => e.Deviceid)
                .HasMaxLength(16)
                .HasColumnName("deviceid");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.Patientname)
                .HasMaxLength(16)
                .HasColumnName("patientname");
            entity.Property(e => e.Sex)
                .HasDefaultValue(0)
                .HasColumnName("sex");
            entity.Property(e => e.Weight).HasColumnName("weight");

            entity.HasOne(d => d.Device).WithMany(p => p.Patients)
                .HasForeignKey(d => d.Deviceid)
                .HasConstraintName("patients_devices_fk");
        });

        modelBuilder.Entity<Rawdatum>(entity =>
        {
            entity.HasKey(e => e.Rawid).HasName("rawdata_pkey");

            entity.ToTable("rawdata");

            entity.HasIndex(e => new { e.Deviceid, e.Createdat }, "rawdata_deviceid_idx").IsDescending(false, true);

            entity.Property(e => e.Rawid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("rawid");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Data)
                .HasColumnType("character varying")
                .HasColumnName("data");
            entity.Property(e => e.Deviceid)
                .HasMaxLength(20)
                .HasColumnName("deviceid");
            entity.Property(e => e.Devicetype)
                .HasMaxLength(20)
                .HasColumnName("devicetype");
            entity.Property(e => e.Frameid).HasColumnName("frameid");
        });

        modelBuilder.Entity<Recorddatum>(entity =>
        {
            entity.HasKey(e => new { e.Deviceid, e.Recordtime }).HasName("recorddata_pkey");

            entity.ToTable("recorddata");

            entity.HasIndex(e => new { e.Deviceid, e.Recordtime }, "recorddata_deviceid_idx").IsDescending(false, true);

            entity.Property(e => e.Deviceid)
                .HasMaxLength(20)
                .HasColumnName("deviceid");
            entity.Property(e => e.Recordtime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("recordtime");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Frameid).HasColumnName("frameid");
            entity.Property(e => e.Statusid)
                .HasMaxLength(5)
                .HasColumnName("statusid");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Sectionid).HasName("section_pkey");

            entity.ToTable("section");

            entity.Property(e => e.Sectionid)
                .ValueGeneratedNever()
                .HasColumnName("sectionid");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey_1");

            entity.ToTable("users");

            entity.Property(e => e.Userid)
                .HasMaxLength(16)
                .HasComment("使用者ID")
                .HasColumnName("userid");
            entity.Property(e => e.Email)
                .HasMaxLength(128)
                .HasComment("使用者Email")
                .HasColumnName("email");
            entity.Property(e => e.Lastlogin)
                .HasComment("最後登入時間")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastlogin");
            entity.Property(e => e.Password)
                .HasMaxLength(16)
                .HasComment("使用者密碼")
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasDefaultValue(2)
                .HasComment("0 = Administrator(A);1 = Engineer(E);2 = User(U)")
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(16)
                .HasComment("使用者名稱")
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
