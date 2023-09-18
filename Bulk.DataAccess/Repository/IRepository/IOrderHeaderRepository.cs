﻿using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulk.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository :IRepository<OrderHeader>
    {
        void Update (OrderHeader orderHeader);    
        void UpdateSatuts (int id , string orderStatus,string? paymentStatus = null);
        void UpdateStripePaymentID (int id , string sessionId,string PaymentIntentId);
    }
}
