using EntityFramework.DbEntities;
using EntityFramework.DbEntities.Chats;
using EntityFramework.Repository;
using EntityFramework.Repository.Chats;
using EntityFramework.Repository.Pictures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjectModel.AuthModel;
using ProjectModel.ChatModels;
using TeamManagementProject_Backend.Controllers.Hubs;
using TeamManagementProject_Backend.Global;

namespace TeamManagementProject_Backend.Controllers
{
    [ApiController]
    [Route("api/user/chat")]
    public class UserChatController : ControllerBase
    {
        //private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatRepository _chatRepository;
        private readonly IPicturesRepository _picturesRepository;
        private readonly UserManager<CustomUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public UserChatController(IChatRepository chatRepository
            , IPicturesRepository pictureRepository
            , UserManager<CustomUser> userManager
            , IHubContext<ChatHub> hubContext) 
        {
            _chatRepository = chatRepository;
            _picturesRepository = pictureRepository;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [AllowAnonymous]
        [Route("SendMessage")]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageModel chatModel)
        {
            if (chatModel == null)
            {
                throw new("Chat is this real ?");
            }
            
            long chatSessionid = chatModel.ChatSessionId;

            if (chatSessionid == 0)
            {
                ChatSession chatSession = new ChatSession
                {
                    FirstUserId = chatModel.SentId,
                    SecondUserId = chatModel.ReceivedId,
                    CreatedDate = DateTime.Now,
                };
                chatSessionid = await _chatRepository.AddSessionAndGetId(chatSession);
            }

            ChatMessages chat = new ChatMessages
            {
                ChatSessionId = chatSessionid,
                ChatMessage = chatModel.Message,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            await _chatRepository.AddMessages(chat);

            var list = await _chatRepository.GetAll();
            
            try {
                await _hubContext.Clients.User(chatModel.ReceivedId).SendAsync("MessageListener", chatModel.SentId, chatModel.Message);

            } catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
            return Ok();
        }

        [AllowAnonymous]
        [Route("GetRecentChatUser")]
        [HttpGet]
        public async Task<IActionResult> GetRecentChatUser(string userId)
        {
            IEnumerable<ChatSession> recentChatSession = await _chatRepository.GetRecentChatSession(userId, 1);
            // List<UserDisplay> recentUser = new List<UserDisplay>();

            // foreach (ChatSession chatSession in recentChatSession) {
            //     try {
            //         CustomUser user = new CustomUser();
            //         string picture = "";
                    
            //         if (chatSession.FirstUserId == userId) {
            //             user = await _userManager.Users.FirstAsync(e => e.Id == chatSession.SecondUserId);
            //         } else if (chatSession.SecondUserId == userId) {
            //             user = await _userManager.Users.FirstAsync(e => e.Id == chatSession.FirstUserId);
            //         }
            //         picture = await _picturesRepository.GetProfilePicture(user.Id);
                    
            //         recentUser.Add(new UserDisplay{
            //             UserId = user.Id,
            //             Email = user.Email,
            //             ChatSessionId = chatSession.Id,
            //             UserName = user.UserName,
            //             UserProfile = picture,
            //             Role = "User"
            //         });
            //     } catch (Exception) {
            //         continue;
            //     }
            // }

            return Ok(recentChatSession);
        }

        [Authorize]
        [Route("GetRecentChatMessage")]
        [HttpGet]
        public async Task<IActionResult> GetRecentChatMessage(string chatSessionId) 
        {

            throw new NotImplementedException();
        }

        [AllowAnonymous]
        [Route("GetUploadFolder")]
        [HttpGet]
        public IActionResult GetUploadFolder()
        {
            return Ok(AppFolders.UserProfilePictures);
        }

        [AllowAnonymous]
        [Route("ReadMessages")]
        [HttpGet]
        public async Task<IActionResult> ReadMessages(long chatSessionId) {
            await _chatRepository.ReadMessage(chatSessionId); 
            return Ok();
        }

        [Route("SearchUsers")]
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchValues) {
            var users = await _userManager.Users.Where(user => user.UserName.Contains(searchValues) || user.Email.Contains(searchValues) || user.Id.Contains(searchValues)).ToListAsync();
            // var users = await _userManager.Users.Where(user => user.UserName.Contains(searchValues)).ToListAsync();
            List<UserDisplay> userDisplays = new List<UserDisplay>();

            foreach (var user in users) {
                try {
                    string picture = await _picturesRepository.GetProfilePicture(user.Id);
                    userDisplays.Add(new UserDisplay{
                                UserId = user.Id,
                                Email = user.Email,
                                UserName = user.UserName,
                                UserProfile = picture,
                                Role = "User",
                             });
                } catch (Exception) {
                    continue;
                }
            }

            return Ok(userDisplays);
        }
    }
}
