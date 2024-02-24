using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface ITenderBidService
    {
        Task<ResponseModel<List<TenderBid>>> GetTenderBidsByTenderId(int TenderId);
    }
}
