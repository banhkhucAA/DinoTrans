using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;
        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }
    }
}
