using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.TenderSteps
{
    public class CreateTenderStep1DTO
    {
        public DateOnly TenderStartDate { get; set; }
        public DateOnly TenderEndDate { get; set; }
        public TimeOnly TenderStartTime { get; set; }
        public TimeOnly TenderEndTime { get; set; }
    }
}
