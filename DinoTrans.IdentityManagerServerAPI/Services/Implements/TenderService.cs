using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.Contracts;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderService : ITenderService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ICompanyRepository _companyRepository;

        public TenderService(ITenderRepository tenderRepository,
            ICompanyRepository companyRepository)
        {
            _tenderRepository = tenderRepository;
            _companyRepository = companyRepository;
        }

        public async Task<ResponseModel<Tender>> CreateTenderStep1(CreateTenderStep1DTO dto)
        {
            var companyShipper = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == dto.CompanyShipperId)
                .FirstOrDefault();
            if(companyShipper == null ) { }
            var newTender = new Tender()
            {
                TenderStatus = TenderStatuses.Draft,
                CompanyShipperId = dto.CompanyShipperId,
                Name = dto.TenderName,
                StartDate = dto.TenderStartDate,
                EndDate = dto.TenderEndDate
            };
            _tenderRepository.Add(newTender);
            _tenderRepository.SaveChange();
            newTender.CompanyShipper = companyShipper;
            return new ResponseModel<Tender>
            {
                Success = true,
                Data = newTender
            };
        }
    }
}
