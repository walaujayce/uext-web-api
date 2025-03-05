using AutoMapper;
using UMonitorWebAPI.Dtos;
using UMonitorWebAPI.Models;

namespace UMonitorWebAPI.Profiles
{
    public class UMonitorProfile: Profile
    {
        public UMonitorProfile()
        {
            // Tell AutoMapper to map Alert to AlertGetDto
            CreateMap<Alert, AlertGetDto>();
            CreateMap<AlertPostDto, Alert>();
            CreateMap<AlertPutDto, Alert>();

            CreateMap<Device, DeviceGetDto>();
            CreateMap<DevicePostDto, Device>();
            CreateMap<DevicePutDto, Device>();
            CreateMap<DeviceStatusPutDto, Device>();

            CreateMap<Patient, PatientGetDto>();
            CreateMap<PatientPostDto, Patient>();
            CreateMap<PatientPutDto, Patient>();

            CreateMap<User, UserGetDto>();
            CreateMap<UserPostDto, User>();
            CreateMap<UserPutDto, User>();

            CreateMap<Notification, NotificationGetDto>();
            CreateMap<NotificationPostDto, Notification>();
            CreateMap<NotificationPutDto, Notification>();

            CreateMap<Rawdatum, RawdatumGetDto>();

            CreateMap<Recorddatum, RecordDataGetDto>();

            CreateMap<Errorlog, ErrorlogGetDto>();
            CreateMap<ErrorlogPutDto, Errorlog>();



        }
    }
}
