// File: Controllers/EncountersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.DAL; // Thêm DAL
using QuanLyPhongKhamApi.Models;
using System.Security.Claims;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EncountersController : ControllerBase
    {
        private readonly EncounterBLL _bus;
        private readonly DoctorDAL _doctorDal; // Thêm DoctorDAL để kiểm tra quyền

        public EncountersController(EncounterBLL bus, DoctorDAL doctorDal)
        {
            _bus = bus;
            _doctorDal = doctorDal;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        public IActionResult GetAll()
        {
            return Ok(_bus.GetAllEncounters());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        public IActionResult GetById(int id)
        {
            var encounter = _bus.GetEncounterById(id);
            if (encounter == null) return NotFound();

            // ✅ HOÀN THIỆN: Phân quyền chi tiết cho bác sĩ
            if (User.IsInRole("Doctor"))
            {
                var accountId = int.Parse(User.FindFirstValue("AccountID")!);
                var doctorProfile = _doctorDal.GetByAccountId(accountId);

                // Nếu bác sĩ không có hồ sơ hoặc không phải là người thực hiện lần khám này, cấm truy cập
                if (doctorProfile == null || doctorProfile.DoctorID != encounter.DoctorID)
                {
                    return Forbid(); // Trả về lỗi 403 Forbidden
                }
            }

            return Ok(encounter);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            // ✅ DỌN DẸP: Xóa try-catch
            bool result = _bus.DeleteEncounter(id);
            if (!result) return NotFound();
            return NoContent(); // Sử dụng 204 No Content cho thao tác xóa thành công
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost("complete")]
        public IActionResult CompleteEncounter([FromBody] CompleteEncounterRequest request)
        {
            // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
            var doctorAccountIdStr = User.FindFirstValue("AccountID");
            if (!int.TryParse(doctorAccountIdStr, out int doctorAccountId))
            {
                return Unauthorized(new { message = "Không thể xác định ID Bác sĩ." });
            }

            var newEncounterId = _bus.CompleteEncounter(doctorAccountId, request);

            return Ok(new
            {
                message = "Hoàn tất lần khám thành công.",
                EncounterID = newEncounterId
            });
        }
    }
}