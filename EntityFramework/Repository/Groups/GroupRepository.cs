using EntityFramework.DbEntities.Groups;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository.Groups
{
    public class GroupRepository : IGroupRepository
    {
        private ProjectDbContext _dbContext;

        public GroupRepository(ProjectDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Group entity)
        {
            await _dbContext.Groups.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddUserToGroup(GroupUsers groupUser)
        {
            await _dbContext.GroupUsers.AddAsync(groupUser);
            await _dbContext.SaveChangesAsync();
        }

        public void Delete(Group entity)
        {
            _dbContext.Groups.Remove(entity);
            _dbContext.SaveChanges();
        }

        public async Task<Group> Get(string id)
        {
            Group group = await _dbContext.Groups.FirstOrDefaultAsync(e => e.Id == id);
            
            return group;
        }

        public async Task<IEnumerable<Group>> GetAllByUserId(string UserId)
        {
            List<GroupUsers> groupUsers = await _dbContext.GroupUsers.Where(e => e.UserId.Equals(UserId)).ToListAsync();
            List<Group> groups = new List<Group>();

            foreach (GroupUsers groupUser in groupUsers)
            {
                Group group = _dbContext.Groups.FirstOrDefault(e => e.Id.Equals(groupUser.GroupId));
                groups.Add(group);
            }
            
            return groups;
        }

        public async Task Update(Group entity, string id)
        {
            Group group = await _dbContext.Groups.FirstOrDefaultAsync(e => e.Id == id);
            group.Name = entity.Name;
            group.password = entity.password;
            group.IsPublic = entity.IsPublic;
            group.IconUrl = entity.IconUrl;
            group.Description = entity.Description;

            _dbContext.Groups.Update(group);
            _dbContext.SaveChanges();
        }
    }
}
