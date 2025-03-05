using Swashbuckle.AspNetCore.Filters;

namespace UMonitorWebAPI.Dtos
{
    public class RecordDataFilterRequestBodyExample : IExamplesProvider<RecordDataFilterRequest>
    {
        public RecordDataFilterRequest GetExamples()
        {
            return new RecordDataFilterRequest
            {
                Deviceid = "string",
                StartTime = DateTime.Parse("2025-01-01T00:00:00"),
                EndTime = DateTime.Parse("2025-01-01T00:00:00") 
            };
        }
    }
    public class RawdatumFilterRequestBodyExample : IExamplesProvider<RawdatumFilterRequest>
    {
        public RawdatumFilterRequest GetExamples()
        {
            return new RawdatumFilterRequest
            {
                Deviceid = "string",
                StartTime = DateTime.Parse("2025-01-01T00:00:00"),
                EndTime = DateTime.Parse("2025-01-01T00:00:00") 
            };
        }
    }
    public class ErrorlogFilterRequestBodyExample : IExamplesProvider<ErrorlogFilterRequest>
    {
        public ErrorlogFilterRequest GetExamples()
        {
            return new ErrorlogFilterRequest
            {
                Deviceid = "string",
                StartTime = DateTime.Parse("2025-01-01T00:00:00"),
                EndTime = DateTime.Parse("2025-01-01T00:00:00")
            };
        }
    }
}
