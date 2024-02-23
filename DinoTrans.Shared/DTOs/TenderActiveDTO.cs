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
        public string PickUpDate { get; set; }
        public string DeliveryDate { get; set; }    
        public string Status { get; set; }
        public float TimeRemaining { get; set; }
        public int Bids { get; set; }
    }
}
