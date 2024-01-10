using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    public class ServiceResponses
    {
        public record GeneralResponse(bool Flag, string Message); //Định nghĩa lớp dữ liệu không thay đổi với các thuộc tính
        public record LoginResponse(bool Flag, string Token, string Message);
    }
}
