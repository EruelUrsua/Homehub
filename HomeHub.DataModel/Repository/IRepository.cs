using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeHub.DataModel.Repository
{
    public interface IRepository<T>
    {
        Task<T> GetByIdAsync(int clientId);
        Task<List<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }

    public class OrderRepository : IRepository<ClientOrder>
    {
        private readonly HomeHubContext _context;

        public OrderRepository(HomeHubContext context)
        {
            _context = context;
        }

        public async Task<ClientOrder> GetByIdAsync(int clientId)
        {
            return await _context.ClientOrders.FindAsync(clientId);
        }

        public async Task<List<ClientOrder>> GetAllAsync()
        {
            try
            {
                return await _context.ClientOrders.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                throw new Exception("Error retrieving orders from database", ex);
            }
        }

        public async Task AddAsync(ClientOrder order)
        {
            _context.ClientOrders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ClientOrder order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ClientOrder order)
        {
            _context.ClientOrders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}
