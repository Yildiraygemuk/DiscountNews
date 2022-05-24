using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountNews.Communication
{
    public interface IEmailService
    {
        void SendEmail(string email,string productName,string url);
    }
}
