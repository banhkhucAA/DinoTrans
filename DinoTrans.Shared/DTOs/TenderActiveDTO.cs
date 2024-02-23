using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class TenderActiveDTO
    {
        public int TenderId { get; set; }
        public string TenderName { get; set; }
        public List<Entities.ContructionMachine> ConstructionMachines { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime PickUpDate { get; set; }
        public DateTime DeliveryDate { get; set; }    
        public string Status { get; set; }
        public float TimeRemaining { get; set; }
        public int Bids { get; set; }
        public int CompanyShipperId { get; set; }
        public string CompanyShipperName { get; set; }
    }
}
