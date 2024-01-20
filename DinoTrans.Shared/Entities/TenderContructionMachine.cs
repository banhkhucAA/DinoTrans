using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class TenderContructionMachine
    {
        public int Id { get; set; }
        public int TenderId { get; set; }
        public int ContructionMachineId { get; set; }
        public DateTime PickUpDate { get; set; }
        public DateTime DeiliverDate { get; set; }
        public string PickUpAddress { get; set; }
        public string PickUpContact { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryContact { get; set; }
        public string? ContructionMachineNotes { get; set; }
        [ForeignKey("TenderId")]
        public virtual Tender? Tender { get; set; }
        [ForeignKey("ContructionMachineId")]
        public virtual ContructionMachine? ContructionMachine { get;set; }
    }
}
