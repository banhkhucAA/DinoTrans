using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Implements;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderConstructionMachineService : ITenderConstructionMachineService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderConstructionMachineRepository _tendercontructionMachineRepository;

        public TenderConstructionMachineService(
            ITenderRepository tenderRepository,
            ITenderConstructionMachineRepository tendercontructionMachineRepository)
        {
            _tenderRepository = tenderRepository;
            _tendercontructionMachineRepository = tendercontructionMachineRepository;
        }

        public async Task<GeneralResponse> CreateTenderConstructionMachine(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto)
        {
            try
            {
                // Kiểm tra xem có máy xây dựng nào được chọn không
                if (dto.ConstructionMachineIds == null||!dto.ConstructionMachineIds.Any())
                {
                    return new GeneralResponse(false, "Vui lòng chọn ít nhất một máy xây dựng");
                }

                // Kiểm tra xem TenderId có tồn tại không
                var existingTender = _tenderRepository
                .AsNoTracking()
                .Where(t => t.Id == dto.TenderId)
                .FirstOrDefault();
                
                if (existingTender == null)
                {
                    return new GeneralResponse(false, "Không tìm thấy thông tin thầu");
                }
                // Cập nhật thông tin của Tender
                existingTender.PickUpDate = dto.PickUpDateAndTime;
                existingTender.DeiliverDate = dto.DeliveryDateAndTime;
                existingTender.PickUpAddress = dto.PickUpAddress;
                existingTender.DeliveryAddress = dto.DeliveryAddress;
                existingTender.PickUpContact = dto.ContactAtPickUpAddress;
                existingTender.DeliveryContact = dto.ContactAtDeliveryAddress;
                existingTender.ContructionMachineNotes = dto.Notes;
                existingTender.Documentations = dto.Documentations;

                _tenderRepository.Update(existingTender);
                _tenderRepository.SaveChange();
                // Lặp qua danh sách ConstructionMachineIds và thêm từng máy xây dựng vào TenderContructionMachine
                foreach (var constructionMachineId in dto.ConstructionMachineIds)
                {
                    // Kiểm tra xem đã tồn tại bản ghi với tenderId và constructionMachineId tương ứng chưa
                    var existingRecord = _tendercontructionMachineRepository
                        .AsNoTracking()
                        .Where(tc => tc.TenderId == dto.TenderId && tc.ContructionMachineId == constructionMachineId)
                        .FirstOrDefault();

                    if (existingRecord != null)
                    {
                        return new GeneralResponse(false, $"Đã tồn tại bản ghi với TenderId {dto.TenderId} và ContructionMachineId {constructionMachineId}");
                    }

                    var newTenderContructionMachine = new TenderContructionMachine
                    {
                        TenderId = dto.TenderId,
                        ContructionMachineId = constructionMachineId,
                    };
                    _tendercontructionMachineRepository.Add(newTenderContructionMachine);
                }

                _tendercontructionMachineRepository.SaveChange();

                return new GeneralResponse(true, "Thêm máy vào thầu thành công");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Lỗi khi thêm máy vào thầu: {ex.Message}");
            }
        }
    }
}
