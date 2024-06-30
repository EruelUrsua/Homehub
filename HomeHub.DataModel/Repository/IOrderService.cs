using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeHub.DataModel.Repository
{
    public interface IOrderService
    {
        Task AcceptOrderAsync(ClientOrder order);
    }

    public class OrderService : IOrderService
    {
        private readonly IRepository<ClientOrder> _orderRepository;

        public OrderService(IRepository<ClientOrder> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task AcceptOrderAsync(ClientOrder order)
        {
            await _orderRepository.AddAsync(order);
        }
    }
}
