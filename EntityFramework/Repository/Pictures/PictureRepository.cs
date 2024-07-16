using EntityFramework.DbEntities.Pictures;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository.Pictures
{
    public class PictureRepository : IPicturesRepository
    {
        private ProjectDbContext _dbContext;

        public PictureRepository(ProjectDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task AddGroupProfile(GroupProfilePicture picture)
        {
            await _dbContext.GroupProfilePicture.AddAsync(picture);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddUserProfile(UserProfilePicture picture)
        {
            await _dbContext.UserProfilePicture.AddAsync(picture);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> GetProfilePicture(string userId) {
            UserProfilePicture profilePicture = await _dbContext.UserProfilePicture.FirstOrDefaultAsync(e => e.UserId == userId);
            if (profilePicture == null) 
            {
                return "";
            }
            return profilePicture.Url;
        }

        public async Task<List<UserProfilePicture>> GetAll() {
            List<UserProfilePicture> userProfile = await _dbContext.UserProfilePicture.ToListAsync();
            return userProfile;
        }
    }
}
