using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderBidService : ITenderBidService
    {
        private readonly ITenderBidRepository _tenderBidRepository;
        public TenderBidService(ITenderBidRepository tenderBidRepository) 
        {
            _tenderBidRepository = tenderBidRepository;
        }
        public async Task<ResponseModel<List<TenderBid>>> GetTenderBidsByTenderId(int TenderId)
        {
            var tenderBids = await _tenderBidRepository
                .AsNoTracking()
                .Where(t => t.TenderId == TenderId)
                .ToListAsync();

            if(tenderBids == null)
            {
                return new ResponseModel<List<TenderBid>>
                {
                    Success = false,
                    Message = "Can't get TenderBids"
                };
            }    
            return new ResponseModel<List<TenderBid>>
            {
                Success = false,
                Data = tenderBids
            };
        }
    }
}
