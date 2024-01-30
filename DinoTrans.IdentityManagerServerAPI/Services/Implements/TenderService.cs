﻿using DinoTrans.Shared.DTOs;
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

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderService : ITenderService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IContructionMachineRepository _contructionMachineRepository;

        public TenderService(ITenderRepository tenderRepository,
            ICompanyRepository companyRepository, 
            IContructionMachineRepository contructionMachineRepository)
        {
            _tenderRepository = tenderRepository;
            _companyRepository = companyRepository;
            _contructionMachineRepository = contructionMachineRepository;
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

        public async Task<GeneralResponse> CreateContructionMachine(CreateContructionMachineDTO dto)
        {
            if (dto is null) return new GeneralResponse(false, "Model is empty");
            try
            {
                // Kiểm tra xem máy đã có trong cơ sở dữ liệu hay chưa
                var existingContructionMachine = _contructionMachineRepository
                    .AsNoTracking()
                    .Where(c => c.SerialNumber == dto.SerialNumber)
                    .FirstOrDefault();

                if (existingContructionMachine != null)
                {
                    return new GeneralResponse(false, "Máy đã tồn tại");
                }

                //Kiểm tra công ty có tồn tại không
                var findCompany = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == dto.CompanyShipperId)
                .FirstOrDefault();
                if (findCompany == null)
                {
                    return new GeneralResponse(false, "Company not found");
                }

                // Tiếp tục kiểm tra Role 
                if (findCompany.Role == CompanyRoleEnum.Carrier)
                {
                    return new GeneralResponse(false, "Forbidden");
                }

                // Thêm máy mới
                var newContructionMachine = new ContructionMachine
                {
                    Name = dto.Name,
                    Brand = dto.Brand,
                    SerialNumber = dto.SerialNumber,
                    Image = dto.Image,
                    Height = dto.Height,
                    Weight = dto.Weight,
                    Length = dto.Length,
                    Width = dto.Width,
                    CompanyShipperId = dto.CompanyShipperId
                };
                _contructionMachineRepository.Add(newContructionMachine);
                _contructionMachineRepository.SaveChange();

                return new GeneralResponse(true, "Thêm mới máy thành công");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }

    }
}
