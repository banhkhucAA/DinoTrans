using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class TenderBidClientService : ITenderBidService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/TenderBid";

        // Constructor nhận các dependency thông qua dependency injection
        public TenderBidClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }

        public Task<ResponseModel<List<TenderBid>>> GetTenderBidsByTenderId(int TenderId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponses.GeneralResponse> SubmitTenderBid(TenderBidDTO dto, ApplicationUser currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/SubmitTenderBid",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ServiceResponses.GeneralResponse(false, "Lỗi xảy ra");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ServiceResponses.GeneralResponse>(apiResponse);
        }
    }
}
