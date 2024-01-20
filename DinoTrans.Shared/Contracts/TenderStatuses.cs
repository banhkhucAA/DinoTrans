using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Contracts
{
    public enum TenderStatuses
    {
        /// <summary>
        /// Before Tender start
        /// </summary>
        Draft = 1,
        /// <summary>
        /// Tender start, current date in Tender period
        /// </summary>
        /// carrier đặt giá trong thời gian này còn shipper hết thời gian này mới chọn carrier thắng, khi đc đặt giá màn shipper vẫn là active còn thằng 
        /// Shipper màn active tender luôn xuất hiện trong khoảng thời gian đặt giá
        /// Carrier chỉ cần đặt một giá là nó sẽ từ active chuyển sang màn selection
        Active = 2, 
        /// <summary>
        /// Tender period end, tender move to To Assign
        /// </summary>
        /// Đối với màn shipper có tender bid và chọn carrier
        /// chỉ cho shipper khi hết thời gian đấu thầu nó sẽ là màn selection, carrier có màn selection nhưng không phải trạng thái toasign mà nó dựa vào bản ghi trong tenderbid
        ToAssign = 3,
        /// <summary>
        /// Tender withdrawn
        /// </summary>
        /// Shipper khi mà hết thời gian đấu thầu không có Carrier nào đặt giá
        /// Khi cả hai bên đã ở trạng thái inexcecution 
        Withdrawn = 4,
        /// <summary>
        /// Tender assigned, transport in execution
        /// </summary>
        /// Khi carrier đặt giá 
        InExcecution = 5,
        /// <summary>
        /// Tender marked as complete by both company
        /// </summary>
        Completed = 6
    }
}
