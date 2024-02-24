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
                        || c.ConstructionMachines.Any(cm => cm.Name.Contains(dto.SearchText!))
                        && (
                            (dto.searchLoads == SearchActiveByMachines.All)
                            || (dto.searchLoads == SearchActiveByMachines.LessThan8Tons && c.ConstructionMachines.Any(cm => cm.Weight < 8000))
                            || (dto.searchLoads == SearchActiveByMachines.From8To22Tons && c.ConstructionMachines.Any(cm => cm.Weight >= 8000 && cm.Weight < 22000))
                            || (dto.searchLoads == SearchActiveByMachines.MoreThan8Tons && c.ConstructionMachines.Any(cm => cm.Weight >= 22000))
                            )
                        && (
                            (dto.searchOffers == SearchActiveByOffers.All)
                            || (dto.searchOffers == SearchActiveByOffers.NoOffers && c.Bids == 0)
                            || (dto.searchOffers == SearchActiveByOffers.Max5Offers && c.Bids <= 5)
                            || (dto.searchOffers == SearchActiveByOffers.MoreThan5Offers && c.Bids > 5)
                            )
                        );

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
                .AsNoTracking()
                .Where(t => TenderIds.Contains(t.Id))
                .ToListAsync();

            foreach(var item in listTenders)
            {
                item.TenderStatus = TenderStatuses.Withdrawn;
            }

            _tenderRepository.UpdateRange(listTenders);
            _tenderRepository.SaveChange();
            return new GeneralResponse(true, "Cập nhật thành công");
        }
    }
}
