using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface ITenderService
    {
        Task<ResponseModel<Tender>> CreateTenderStep1(CreateTenderStep1DTO dto);
        Task<ResponseModel<Tender>> CreateTenderStep2(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto);
        Task<ResponseModel<Tender>> StartTender(int TenderId);
        Task<ResponseModel<TenderActiveDTO>> SearchActiveBy (SearchTenderActiveDTO dto, ApplicationUser currentUser);
    }
}
