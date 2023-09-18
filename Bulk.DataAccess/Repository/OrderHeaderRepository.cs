using Bulk.DataAccess.Data;
using Bulk.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulk.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private  AppDbcontext  _db;
        public OrderHeaderRepository(AppDbcontext db) : base(db)
        {
            _db = db;   
        }

        

        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateSatuts(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDB = _db.OrderHeaders.FirstOrDefault(e=>e.Id == id);
            if(orderFromDB != null) 
            {
                orderFromDB.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDB.PaymentStatus = paymentStatus;  
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string PaymentIntentId)
        {
            var orderFromDB = _db.OrderHeaders.FirstOrDefault(e => e.Id == id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderFromDB.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(PaymentIntentId))
            {
                orderFromDB.PaymentInteendId = PaymentIntentId;
                orderFromDB.PaymentDate = DateTime.Now;
            }
        }
    }
}
