using System;
using System.Collections.Generic;
using System.Text;

namespace Covid_Notification
{
    public class CovidResponseModel
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Fee_type { get; set; }
        public IEnumerable<Session> Sessions { get; set; }
        public IEnumerable<VaccineFees> Vaccine_fees {get;set;}
    }

    public class Session
    {
        public int Available_capacity { get; set; }
        public string Date { get; set; }
        public int Min_age_limit { get; set; }
        public string Vaccine { get; set; }
    }

    public class VaccineFees
    {
        public string vaccine { get; set; }
        public string fee { get; set; }
    }


    public class CreateMessage
    {
        public string mssg { get; set; }
        public int totalCount { get; set; }
    }
}
