﻿using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class ConstructionMachineClientService : IConstructionMachineService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/ConstructionMachine";

        // Constructor nhận các dependency thông qua dependency injection
        public ConstructionMachineClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }
        public async Task<GeneralResponse> CreateContructionMachine(CreateContructionMachineDTO dto)
        {
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/CreateContructionMachine",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(true, "Tạo mới máy thành công");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<SearchConstructionMachineDTO>> SearchConstructionMachineForTender(SearchLoadForTenderDTO dto)
        {
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/SearchConstructionMachineForTender",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
            return new ResponseModel<SearchConstructionMachineDTO>
            {
                Success = false,
                Message = "Can't search construction machine"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<SearchConstructionMachineDTO>>(apiResponse);
        }
    }
}