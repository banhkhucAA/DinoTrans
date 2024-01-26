using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class TenderChangeStepDTO
    {
        public int CurrentStep { get; set; }
        public Tender Tender { get; set; }
    }
}
