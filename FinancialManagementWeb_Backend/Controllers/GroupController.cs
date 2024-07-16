using EntityFramework.DbEntities.Groups;
using EntityFramework.Repository.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectModel.GroupModels;
using System.Text;

namespace TeamManagementProject_Backend.Controllers
{

    [Route("api/group")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _groupRepository;
        
        public GroupController(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        private string RandomString(int length)
        {
            Random random = new Random();
            char offset = 'A';
            var builder = new StringBuilder(length);
            const int lettersOffset = 26;
            for (int i = 0; i < length; i++) 
            {
                var @char = (char)random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return builder.ToString();
        }

        [Route("GetGroupById")]
        [HttpGet]
        public async Task<IActionResult> GetGroupById(string groupId) 
        {
            GetGroupModel groupModel = new GetGroupModel();
            Group group = await _groupRepository.Get(groupId);
            if (group == null)
            {
                return new EmptyResult();
            }
            groupModel.Id = group.Id;
            groupModel.Name = group.Name;
            groupModel.Description = group.Description;
            groupModel.IconUrl = group.IconUrl;
            groupModel.IsPublic = group.IsPublic;
                
            return Ok(groupModel);
        }

        [Route("Add")]
        [HttpPost]
        public async Task<IActionResult> AddGroup([FromBody] AddGroupModel groupModel)
        {
            Group group = new Group();
            group.Id = RandomString(5);
            group.Name = groupModel.Name;
            group.IconUrl = groupModel.IconUrl;
            group.IsPublic = groupModel.IsPublic;
            group.password = groupModel.password;
            group.Description = groupModel.Description;

            GroupUsers groupUsers = new GroupUsers();
            groupUsers.UserAdded = "";
            groupUsers.GroupId = group.Id;
            groupUsers.UserId = groupModel.UserId;
            groupUsers.Role = groupModel.Role;
            groupUsers.DateAdded = DateTime.Now;

            await _groupRepository.Add(group);

            await _groupRepository.AddUserToGroup(groupUsers);

            return Ok(new { Message = "Request Completed" });
        }

        [Route("AddUsers")]
        [HttpPost]
        public async Task<IActionResult> AddUsers([FromBody] AddUserModel userModel)
        {
            GroupUsers groupUsers = new GroupUsers();
            groupUsers.GroupId = userModel.GroupId;
            groupUsers.UserId = userModel.UserId;
            groupUsers.Role = userModel.Role;
            groupUsers.UserAdded = groupUsers.UserAdded;
            groupUsers.DateAdded = DateTime.Now;

            await _groupRepository.AddUserToGroup(groupUsers);

            return Ok(new { Message = "Request Completed" });
        }

        [Route("GetGroupByUserId")]
        [HttpGet]
        public async Task<IActionResult> GetGroupByUserId(string UserId)
        {
            IEnumerable<Group> groups = await _groupRepository.GetAllByUserId(UserId);
            List<GroupListModel> listOfGroupModels = new List<GroupListModel>();
            foreach (Group group in groups)
            {
                GroupListModel model = new GroupListModel();
                model.Name = group.Name;
                model.IconUrl = group.IconUrl;
                model.HasPassword = group.password.IsNullOrEmpty();
                model.IsPublic = group.IsPublic;
                model.Description = group.Description;
                listOfGroupModels.Add(model);
            }

            return Ok(listOfGroupModels);
        }
    }
}
