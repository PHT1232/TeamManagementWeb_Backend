using EntityFramework.DbEntities.Chats;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository.Chats
{
    public class ChatRepository : IChatRepository
    {
        private ProjectDbContext _dbContext;

        public ChatRepository(ProjectDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddMessages(ChatMessages entity)
        {
            await _dbContext.Chats.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<long> AddSessionAndGetId(ChatSession entity)
        {
            await _dbContext.ChatSession.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            long id = entity.Id;

            return id; 
        }

        public void Delete(ChatMessages entity)
        {

            throw new NotImplementedException();
        }

        public async Task<ChatMessages> Get(long id)
        {
            ChatMessages chat = await _dbContext.Chats.FirstOrDefaultAsync(x => x.Id == id);
            return chat;
        }

        public async Task<List<ChatMessages>> GetAll()
        {
            List<ChatMessages> chats = await _dbContext.Chats.ToListAsync();
            return chats;
        }

        // public async Task<IEnumerable<ChatSession>> GetRecentChatSession(string userId)
        // {
        //     int lastRow = await _dbContext.ChatSession.CountAsync();
        //     var lastInt = await _dbContext.ChatSession.LastAsync();
        //     DateTime lastDate = new DateTime().Date;
        //     List<ChatSession> recentUserFromChatRepo = await _dbContext.ChatSession
        //         .OrderBy(e => e.CreatedDate)
        //         .Where(e => e.FirstUserId == userId || e.SecondUserId == userId)
        //         .Take(10)
        //         .ToListAsync();

        //     return recentUserFromChatRepo;
        // }

        public async Task<List<ChatSession>> GetRecentChatSession(string userId, long lastTakeRow)
        {
            List<ChatSession> recentUserFromChatRepo = await _dbContext.ChatSession
                .OrderBy(e => e.CreatedDate)
                .Where(e => e.FirstUserId == userId || e.SecondUserId == userId && e.Id > lastTakeRow)
                .Take(10)
                .ToListAsync();

            return recentUserFromChatRepo;
        }

        public async Task ReadMessage(long id) {
            await _dbContext.Chats
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(z => z.IsRead, true));
        }

        public async Task<List<ChatMessages>> GetRecentChatMessagesUser(long chatSessionId, DateTime lastMessageSentDate)
        {
            List<ChatMessages> chats = await _dbContext.Chats
                .OrderByDescending(e => e.CreatedDate)
                .Where(e => e.ChatSessionId == chatSessionId && e.CreatedDate < lastMessageSentDate)
                .Take(10)
                .ToListAsync();

            return chats;
        }

        public async Task Update(ChatMessages entity, long id)
        {
            throw new NotImplementedException();
        }
    }
}
