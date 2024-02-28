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
        private readonly ITenderRepository _tenderRepository;
        public TenderBidService(ITenderBidRepository tenderBidRepository,
            ITenderRepository tenderRepository) 
        {
            _tenderBidRepository = tenderBidRepository;
            _tenderRepository = tenderRepository;
        }
        public async Task<ResponseModel<List<TenderBid>>> GetTenderBidsByTenderId(int TenderId)
        {
            var tenderBids = await _tenderBidRepository
                .AsNoTracking()
                .Include(t => t.CompanyCarrier)
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

        public async Task<ServiceResponses.GeneralResponse> SubmitTenderBid(TenderBidDTO dto, ApplicationUser currentUser)
        {
            var tender = await _tenderRepository
                .AsNoTracking()
                .Where(t => t.Id == dto.TenderId)
                .FirstOrDefaultAsync();

            if (tender == null)
                return new ServiceResponses.GeneralResponse(false, "Không tìm thấy thầu");

            var tenderBidExist = _tenderBidRepository
                .AsNoTracking()
                .Any(t => t.TenderId == dto.TenderId);

            if (tenderBidExist)
                return new ServiceResponses.GeneralResponse(false, "Công ty của bạn đã đặt giá cho thầu này rồi");
            var newTenderBid = new TenderBid
            {
                TenderId = dto.TenderId,
                CompanyCarrierId = (int)currentUser.CompanyId!,
                TransportPrice = dto.TransportPrice
            };
            _tenderBidRepository.Add(newTenderBid);
            _tenderBidRepository.SaveChange();
            return new ServiceResponses.GeneralResponse(true, "Tạo đặt giá thành công");
        }
    }
}
