using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderService : ITenderService
    {
        private readonly ITenderRepository _tenderRepository;

        public TenderService(ITenderRepository tenderRepository)
        {
            _tenderRepository = tenderRepository;
        }

        public async Task<ResponseModel<Tender>> CreateTenderStep1(CreateTenderStep1DTO dto)
        {
            DateTime startDate = DateTime.Parse($"{dto.TenderStartDate} {dto.TenderStartTime}");
            DateTime endDate = DateTime.Parse($"{dto.TenderEndDate} {dto.TenderEndTime}");

            return new ResponseModel<Tender>
            {

            };
        }
    }
}
