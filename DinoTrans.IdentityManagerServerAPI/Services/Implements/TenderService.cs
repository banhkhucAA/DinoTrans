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

        public TenderService(ITenderRepository tenderRepository,
            ICompanyRepository companyRepository, 
            IConstructionMachineRepository contructionMachineRepository,
            ITenderConstructionMachineRepository tenderConstructionMachineRepository,
            IUnitOfWork unitOfWork)
        {
            _tenderRepository = tenderRepository;
            _companyRepository = companyRepository;
            _contructionMachineRepository = contructionMachineRepository;
            _tenderConstructionMachineRepository = tenderConstructionMachineRepository;
            _unitOfWork = unitOfWork;
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

        public async Task<ResponseModel<TenderActiveDTO>> SearchActiveBy(SearchTenderActiveDTO dto, ApplicationUser currentUser)
        {
            var listActive1 = from t in _tenderRepository.AsNoTracking()
                              where t.TenderStatus == TenderStatuses.Active || t.TenderStatus == TenderStatuses.ToAssign
                              join tc in _tenderConstructionMachineRepository.AsNoTracking() on t.Id equals tc.TenderId
                              join c in _contructionMachineRepository.AsNoTracking() on tc.ContructionMachineId equals c.Id into machines
                              select new TenderActiveDTO
                              {
                                  TenderId = t.Id,
                                  TenderName = t.Name,
                                  ConstructionMachines = machines.ToList(),
                                  From = t.PickUpAddress,
                                  To = t.DeliveryAddress,
                                  PickUpDate = t.PickUpDate.ToString(),
                                  DeliveryDate = t.DeiliverDate.ToString(),
                                  Status = t.TenderStatus.ToString(),
                                  TimeRemaining = 0,
                                  Bids = 0
                              };

            var listActive = _tenderRepository
                .AsNoTracking()
                .Where(t => t.TenderStatus == TenderStatuses.Active || t.TenderStatus == TenderStatuses.ToAssign);

            var currentUserCompany = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == currentUser.CompanyId)
                .FirstOrDefault();

            if(currentUserCompany!.Role == CompanyRoleEnum.Shipper)
            {
                listActive = listActive.Where(c =>            
                    c.CompanyShipperId == currentUser.CompanyId);
            }
            else if(currentUserCompany!.Role == CompanyRoleEnum.Carrier)
            {
                listActive = listActive
                    .Where(c =>  c.TenderStatus == TenderStatuses.Active);
            }
            var constructionMachineIds = _tenderConstructionMachineRepository
                .AsNoTracking()
                .Where(t => listActive.Select(l => l.Id).Contains(t.TenderId))
                .Select(t => t.ContructionMachineId);
            var constructionMachines = 
            listActive = listActive.Where(c => dto.SearchText.IsNullOrEmpty()
                    || c.Name.Contains(dto.SearchText!))
                    .Skip((dto.pageIndex - 1) * dto.pageSize)
                    .Take(dto.pageSize);

            return new ResponseModel<TenderActiveDTO>
            {

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
    }
}
