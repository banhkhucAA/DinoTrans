using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface ITenderService
    {
        Task<ResponseModel<Tender>> CreateTenderStep1(CreateTenderStep1DTO dto);
    }
}
