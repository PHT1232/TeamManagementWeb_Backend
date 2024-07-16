using EntityFramework.DbEntities.Chats;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository.Chats
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatMessages>> GetAll();
        Task<ChatMessages> Get(long id);
        // Task<IEnumerable<ChatSession>> GetRecentChatSession(string userId);
        Task<List<ChatSession>> GetRecentChatSession(string userId, int lastTakeRow);
        Task AddMessages(ChatMessages entity);
        Task<long> AddSessionAndGetId(ChatSession entity);
        Task ReadMessage(long Id);
        Task Update(ChatMessages entity, long id);
        void Delete(ChatMessages entity);
    }
}
