using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs.ContructionMachine;
using static DinoTrans.Shared.DTOs.ServiceResponses;
using DinoTrans.Shared.Repositories.Implements;
using Microsoft.AspNetCore.Identity;
using DinoTrans.Shared.DTOs.UserResponse;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DinoTrans.Shared.DTOs.SearchDTO;
using Microsoft.IdentityModel.Tokens;
using NHibernate.Engine;
using DinoTrans.Shared.DTOs.TendersActive;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderService : ITenderService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IConstructionMachineRepository _contructionMachineRepository;
        private readonly ITenderConstructionMachineRepository _tenderConstructionMachineRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IConstructionMachineService _machineService;
        private readonly ITenderBidService _tenderBidService;

        public TenderService(ITenderRepository tenderRepository,
            ICompanyRepository companyRepository, 
            IConstructionMachineRepository contructionMachineRepository,
            ITenderConstructionMachineRepository tenderConstructionMachineRepository,
            IUnitOfWork unitOfWork,
            ITenderBidRepository tenderBidRepository,
            IConstructionMachineService machineService,
            ITenderBidService tenderBidService)
        {
            _tenderRepository = tenderRepository;
            _companyRepository = companyRepository;
            _contructionMachineRepository = contructionMachineRepository;
            _tenderConstructionMachineRepository = tenderConstructionMachineRepository;
            _unitOfWork = unitOfWork;
            _tenderBidRepository = tenderBidRepository;
            _machineService = machineService;
            _tenderBidService = tenderBidService;
        }

        public async Task<ResponseModel<Tender>> CreateTenderStep1(CreateTenderStep1DTO dto)
        {
            var companyShipper = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == dto.CompanyShipperId)
                .FirstOrDefaultAsync();
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

        public async Task<ResponseModel<Tender>> CreateTenderStep2(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto)
        {
            _unitOfWork.BeginTransaction();
            var tender = await _tenderRepository
                .AsNoTracking()
                .Where(t => t.Id == dto.TenderId)
                .FirstOrDefaultAsync();

            if( tender == null ) 
            {
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = "Can't find tender to update",
                    ResponseCode = "404"
                };
            }

            var listConstructionMachine = _contructionMachineRepository
                .AsNoTracking()
                .Where(c => c.CompanyShipperId == tender.CompanyShipperId);

            foreach(var machineId in dto.ConstructionMachineIds)
            {
                if(!listConstructionMachine.Select(s => s.Id).Contains(machineId))
                {
                    _unitOfWork.Rollback();
                    return new ResponseModel<Tender>
                    {
                        Success = false,
                        Message = $"Can't find machine with Id = {machineId}",
                        ResponseCode = "404"
                    };
                }    
            }
            var tenderConstructionMachineList = new List<TenderContructionMachine>();
            var existedTenderConstructionMachines = await _tenderConstructionMachineRepository
                .AsNoTracking()
                .Where(c => c.TenderId == dto.TenderId)
                .ToListAsync();
            if(existedTenderConstructionMachines.Any())
            {
                _tenderConstructionMachineRepository.DeleteRange(existedTenderConstructionMachines);
            }    
            foreach(var item in dto.ConstructionMachineIds) 
            {
                var newTenderConstructionMachine = new TenderContructionMachine
                {
                    TenderId = dto.TenderId,
                    ContructionMachineId = item
                };
                tenderConstructionMachineList.Add(newTenderConstructionMachine);
            }
            tender.PickUpDate = dto.PickUpDateAndTime;
            tender.DeiliverDate = dto.DeliveryDateAndTime;
            tender.PickUpAddress = dto.PickUpAddress;
            tender.DeliveryAddress = dto.DeliveryAddress;
            tender.PickUpContact = dto.ContactAtPickUpAddress;
            tender.DeliveryContact = dto.ContactAtDeliveryAddress;
            tender.Notes = dto.Notes;
            var listDocs = new List<Dictionary<string, string>>();
            if (dto.Documentations != null && dto.Documentations.Count > 0)
            {
                for (int i = 0; i < dto.Documentations.Count; i++)
                {
                    var newDoc = new Dictionary<string, string>()
                    {
                        {$"Document_{i}",dto.Documentations[i] }
                    };
                    listDocs.Add(newDoc);
                }
                tender.Documentations = JsonConvert.SerializeObject(listDocs);
            }
            try
            {
                _tenderRepository.Update(tender);
                _tenderConstructionMachineRepository.AddRange(tenderConstructionMachineList);
                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
                return new ResponseModel<Tender>
                {
                    Success = true,
                    Data = tender
                };
            }
            catch(Exception ex) 
            {
                _unitOfWork.Rollback();
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseModel<TenderDetailsDTO>> GetTenderById(int Id)
        {
            var result = (from t in _tenderRepository.AsNoTracking().Where(t => t.Id == Id)
                          join tc in _tenderConstructionMachineRepository.AsNoTracking() on t.Id equals tc.TenderId into tenderConstructionMachines
                         let constructionMachines = (from c in _contructionMachineRepository.AsNoTracking()
                                                     join tsResult in tenderConstructionMachines on c.Id equals tsResult.ContructionMachineId
                                                     where tsResult.TenderId == Id
                                                     select new ConstructionMachinesForTendersDTO
                                                     {
                                                         Id = c.Id,
                                                         Name = c.Name,
                                                         Brand = c.Brand,
                                                         SerialNumber = c.SerialNumber,
                                                         CompanyShipperId = c.CompanyShipperId,
                                                         Image = c.Image,
                                                         Length = c.Length,
                                                         Width = c.Width,
                                                         Height = c.Height,
                                                         Weight = c.Weight
                                                     }).ToList()
                         select new TenderDetailsDTO
                         {
                             TenderId = t.Id,
                             TenderName = t.Name,
                             TenderStatus = t.TenderStatus,
                             CompanyShipperId = t.CompanyShipperId,
                             CompanyCarrierId = t.CompanyCarrierId,
                             StartDate = t.StartDate,
                             EndDate = t.EndDate,
                             PickUpDate = t.PickUpDate,
                             DeiliverDate = t.DeiliverDate,
                             PickUpAddress = t.PickUpAddress,
                             PickUpContact = t.PickUpContact,
                             DeliveryAddress = t.DeliveryAddress,
                             DeliveryContact = t.DeliveryContact,
                             Notes = t.Notes,
                             Documentations = t.Documentations,
                             ConstructionMachines = constructionMachines
                         }).FirstOrDefault();

            return new ResponseModel<TenderDetailsDTO>
            {
                Data = result!,
                Success = true
            };
        }

        public async Task<ResponseModel<List<Tender>>> GetTendersActiveForAuto()
        {
            var allTenderActive = await _tenderRepository
                .AsNoTracking()
                .Where(t => t.TenderStatus == TenderStatuses.Active || t.TenderStatus == TenderStatuses.ToAssign)
                .ToListAsync();

            if (allTenderActive != null)
            {
                foreach (var item in allTenderActive)
                {
                    var AnyBids = _tenderBidRepository
                        .AsNoTracking()
                        .Any(t => t.TenderId == item.Id);
                    if (AnyBids)
                        allTenderActive.Remove(item);
                }
            }
            else
            {
                return new ResponseModel<List<Tender>>
                {
                    Success = false
                };
            }    

            return new ResponseModel<List<Tender>>
            {
                Success = true,
                Data = allTenderActive!
            };
        }

        public async Task<ResponseModel<List<TenderActiveDTO>>> SearchActiveBy(SearchTenderActiveDTO dto, ApplicationUser currentUser)
        {
            var listActive = _tenderRepository
            .AsNoTracking()
            .Include(t => t.CompanyShipper)
            .Where(t => t.TenderStatus == TenderStatuses.Active || t.TenderStatus == TenderStatuses.ToAssign)
            .ToList();

            var currentUserCompany = _companyRepository
                                    .AsNoTracking()
                                    .Where(c => c.Id == currentUser.CompanyId)
                                    .FirstOrDefault();

            if (currentUserCompany!.Role == CompanyRoleEnum.Shipper)
            {
                listActive = listActive.Where(t =>
                    t.CompanyShipperId! == currentUser.CompanyId).ToList();
            }
            else if (currentUserCompany!.Role == CompanyRoleEnum.Carrier)
            {
                listActive = listActive
                    .Where(c => c.TenderStatus == TenderStatuses.Active).ToList();
            }

            var listTenderActiveDTO = new List<TenderActiveDTO>();
            foreach (var item in listActive)
            {
                var timeRemains = (item.EndDate - DateTime.Now).TotalSeconds;
                var newTenderActiveDTO = new TenderActiveDTO
                {
                    TenderId = item.Id,
                    TenderName = item.Name,
                    From = item.PickUpAddress,
                    To = item.DeliveryAddress,
                    PickUpDate = (DateTime)item.PickUpDate,
                    DeliveryDate = (DateTime)item.DeiliverDate,
                    Status = item.TenderStatus.ToString(),
                    TimeRemaining = timeRemains > 0 ? timeRemains : 0,
                    CompanyShipperId = item.CompanyShipperId,
                    CompanyShipperName = item.CompanyShipper!.CompanyName
                };

                var constructionMachines = await _machineService.GetMachinesForTenderOverviewByIds(item.Id);
                newTenderActiveDTO.ConstructionMachines = constructionMachines.Data;
                var Bids = await _tenderBidService.GetTenderBidsByTenderId(item.Id);
                newTenderActiveDTO.Bids = Bids.Data.Count();
                listTenderActiveDTO.Add(newTenderActiveDTO);
            }    

            var listActiveNotPaging = listTenderActiveDTO.Where(c => dto.SearchText.IsNullOrEmpty()
                        || c.TenderName.Contains(dto.SearchText!)
                        || c.ConstructionMachines.Any(cm => cm.Name.Contains(dto.SearchText!))); 

            switch(dto.searchLoads)
            {
                case SearchActiveByMachines.All:
                    break;
                case SearchActiveByMachines.LessThan8Tons:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight < 8000));
                    break;
                case SearchActiveByMachines.From8To22Tons:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 8000 &&  c.Weight < 22000));
                    break;
                case SearchActiveByMachines.From22Tons:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 22000));
                    break;
            }    

            switch(dto.searchOffers)
            {
                case SearchActiveByOffers.All:
                    break;
                case SearchActiveByOffers.NoOffers:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.Bids == 0);
                    break;
                case SearchActiveByOffers.MoreThan5Offers:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.Bids > 5);
                    break;
                case SearchActiveByOffers.Max5Offers:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.Bids <= 5);
                    break;
            }    
            var listActivePaging = listActiveNotPaging
                        .Skip((dto.pageIndex - 1) * dto.pageSize)
                        .Take(dto.pageSize);

            return new ResponseModel<List<TenderActiveDTO>>
            {
                Data = listActivePaging.ToList(),
                Success = true,
                Total = listActiveNotPaging.Count(),
                PageCount = listActiveNotPaging.Count() / 10 + 1
            };    
        }

        public async Task<ResponseModel<Tender>> StartTender(int TenderId)
        {
            var tender = await _tenderRepository
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == TenderId);

            if(tender == null )
            {
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = "Can't find tender"
                };
            }    

            if(tender.TenderStatus != TenderStatuses.Draft)
            {
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = "Tender status isn't draft"
                };
            }    

            tender.TenderStatus = TenderStatuses.Active;
            _tenderRepository.Update(tender);
            _tenderRepository.SaveChange();
            return new ResponseModel<Tender>
            {
                Data = tender,
                Success = true
            };
        }

        public async Task<GeneralResponse> UpdateStatusAuto(List<int> TenderIds)
        {
            var listTenders = await _tenderRepository
                .Queryable()
                .Where(t => TenderIds.Contains(t.Id))
                .ToListAsync();

            foreach(var item in listTenders)
            {
                item.TenderStatus = TenderStatuses.Withdrawn;
                item.WithdrawReason = "Auto withdraw by system";
            }

            _tenderRepository.UpdateRange(listTenders);
            _tenderRepository.SaveChange();
            return new GeneralResponse(true, "Cập nhật thành công");
        }

        public async Task<GeneralResponse> UpdateWithdrawTender(WithdrawTenderDTO withdrawTenderDTO)
        {
            var tender = await _tenderRepository
                .Queryable()
                .Where(t => t.Id == withdrawTenderDTO.TenderID)
                .FirstOrDefaultAsync();

            if(tender == null)
            {
                return new GeneralResponse(false, "Không tìm thấy Tender");
            }

            tender.TenderStatus = TenderStatuses.Withdrawn;
            tender.WithdrawReason = withdrawTenderDTO.WithdrawReason;
            _tenderRepository.Update(tender);
            _tenderRepository.SaveChange();
            return new GeneralResponse(true, "Cập nhật thành công");
        }
    }
}
