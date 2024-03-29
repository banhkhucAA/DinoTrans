﻿using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.DTOs.UserResponse;
using static DinoTrans.Shared.DTOs.ServiceResponses;
using AutoMapper.Configuration;
using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.Entities;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    // Lớp triển khai của IUserService cho ứng dụng Blazor WebAssembly
    public class UserClientService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/User";

        // Constructor nhận các dependency thông qua dependency injection
        public UserClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }

        // Phương thức để tạo tài khoản người dùng thông qua API
        public async Task<GeneralResponse> CreateAccount(UserDTO userDTO)
        {
            // Gửi yêu cầu POST đến endpoint API để đăng ký tài khoản
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/register",
                // Tạo đối tượng StringContent từ đối tượng userDTO sau khi chuyển thành chuỗi JSON để gửi qua mạng trong yêu cầu HTTP POST
                Generics.GenerateStringContent(Generics.SerializeObj(userDTO)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, "Error occurred. Try again later...");

            // Đọc nội dung của phản hồi từ yêu cầu HTTP POST
            var apiResponse = await response.Content.ReadAsStringAsync();

            // Chuyển đổi chuỗi JSON (apiResponse) thành đối tượng GeneralResponse sử dụng DeserializeJsonString
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        // Phương thức để đăng nhập vào hệ thống thông qua API
        public async Task<LoginResponse> LoginAccount(LoginDTO loginDTO)
        {
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/login",
                Generics.GenerateStringContent(Generics.SerializeObj(loginDTO)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new LoginResponse(false, null!, "Error occurred. Try again later...");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<LoginResponse>(apiResponse);
        }

        public async Task<ResponseModel<UserInfoResponseDTO>> GetAllUserInfo(GetAllUserInfoDTO userDTO)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetAllUserInfo?UserId={userDTO.UserId}&CompanyId={userDTO.CompanyId}");

            //Read Response
            if (!response.IsSuccessStatusCode) return new ResponseModel<UserInfoResponseDTO>
            {
                Success = false,
                ResponseCode = "500",
                Message = "Internal Server Error"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<UserInfoResponseDTO>>(apiResponse);
        }

        public async Task<GeneralResponse> ChangeUserPassword(ChangePasswordDTO changePasswordDTO)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PutAsync($"{BaseUrl}/ChangePassword",
                Generics.GenerateStringContent(Generics.SerializeObj(changePasswordDTO)));

            //Read Response
            if (!response.IsSuccessStatusCode) return new GeneralResponse(false, "Internal server error");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }
        public async Task<GeneralResponse> UpdateIsAdminConfirm()
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PutAsync($"{BaseUrl}/UpdateAdminConfirm",null);

            //Read Response
            if (!response.IsSuccessStatusCode) return new GeneralResponse(false, "Internal server error");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);

        }
        public async Task<ResponseModel<CompanyRoleEnum>> GetCompanyRole(int CompanyId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetCompanyRole?companyId={CompanyId}");

            //Read Response
            if (!response.IsSuccessStatusCode) return new ResponseModel<CompanyRoleEnum>
            {
                Success = false,
                Message = $"Cant get company Role with Id = {CompanyId}"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<CompanyRoleEnum>>(apiResponse);

        }

        public ResponseModel<ApplicationUser> GetUserById(int UserId)
        {
            throw new NotImplementedException();
        }
    }
}
