using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs.TenderSteps
{
    public class ConvertDateTime
    {
        public DateOnly TenderStartDate {  get; set; }
        public DateOnly TenderEndDate { get; set; }
        public TimeOnly TenderStartTime { get; set; }
        public TimeOnly TenderEndTime { get; set; }
    }

    public class ConvertStep2
    {
        public int TenderId { get; set; }
        public ConvertDateTime DateTimeConvert { get; set; }
        public List<int> ConstructionMachineIds { get; set; }
        public string PickUpAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public string ContactAtPickUpAddress { get; set; }
        public string ContactAtDeliveryAddress { get; set; }
        public string Notes { get; set; }
    }
}
