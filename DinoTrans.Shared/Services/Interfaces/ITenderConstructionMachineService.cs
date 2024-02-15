using DinoTrans.Shared.DTOs.TenderSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface ITenderConstructionMachineService
    {
        Task<GeneralResponse> CreateTenderConstructionMachine(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto);

    }
}
