// File: BLL/InvoiceBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.BLL
{
    public class InvoiceBLL
    {
        private readonly InvoiceDAL _dal;
        public InvoiceBLL(InvoiceDAL dal) { _dal = dal; }

        public List<InvoiceDTO> GetAll(string? status) => _dal.GetAll(status);

        // ✅ BỔ SUNG: Thêm hàm GetById để khắc phục lỗi
        public InvoiceDTO? GetById(int id) => _dal.GetById(id);

        public bool ProcessPayment(int id, string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Phương thức thanh toán không được để trống.");
            }
            return _dal.UpdatePaymentStatus(id, paymentMethod);
        }

        // ✅ BỔ SUNG:
        public InvoiceDetailDTO? GetDetailById(int id) => _dal.GetDetailById(id);
    }
}