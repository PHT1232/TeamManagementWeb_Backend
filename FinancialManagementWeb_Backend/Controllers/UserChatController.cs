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
using Microsoft.IdentityModel.Tokens;
using ProjectModel.AuthModel;
using ProjectModel.ChatModels;
using TeamManagementProject_Backend.Controllers.Hubs;
using TeamManagementProject_Backend.Global;
using TeamManagementProject_Backend.Helpers;

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

        //[AllowAnonymous]
        //[Route("SendMessage")]
        //[HttpPost]
        //public async Task<IActionResult> SendMessage([FromBody] ChatMessageInsertModel chatModel)
        //{
        //    if (chatModel == null)
        //    {
        //        throw new("Chat is this real ?");
        //    }
            
        //    long chatSessionid = chatModel.ChatSessionId;

        //    if (chatSessionid == 0)
        //    {
        //        ChatSession chatSession = new ChatSession
        //        {
        //            FirstUserId = chatModel.SentId,
        //            SecondUserId = chatModel.ReceivedId,
        //            CreatedDate = DateTime.Now,
        //        };
        //        chatSessionid = await _chatRepository.AddSessionAndGetId(chatSession);
        //    }

        //    ChatMessages chat = new ChatMessages
        //    {
        //        ChatSessionId = chatSessionid,
        //        SentUserId = chatModel.SentId,
        //        ChatMessage = chatModel.Message,
        //        CreatedDate = DateTime.Now,
        //        IsRead = false
        //    };

        //    ChatMessages chatDb = await _chatRepository.AddMessagesAndGetData(chat);

        //    try {
        //        ChatMessageModel chatMessageModel = new ChatMessageModel
        //        {
        //            Id = chatDb.Id,
        //            SentUserId = chatDb.SentUserId,
        //            ChatMessage = chatDb.ChatMessage,
        //            CreatedDate = chatDb.CreatedDate,
        //        };
        //        await _hubContext.Clients.User(chatModel.ReceivedId).SendAsync("MessageListener", chatMessageModel);
        //    } catch (Exception ex) {
        //        throw new Exception(ex.ToString());
        //    }
        //    return Ok();
        //}

        [AllowAnonymous]
        [Route("SendMessage")]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageInsertModel chatModel)
        {
            if (chatModel == null)
            {
                throw new("Chat is this real ?");
            }
            ChatMessageModel chatMessageModel = new ChatMessageModel
            {
                Id = chatModel.ChatSessionId,
                SentUserId = chatModel.SentId,
                ChatMessage = chatModel.Message,
                CreatedDate = DateTime.Now,
            };
            try {
                await _hubContext.Clients.User(chatModel.ReceivedId).SendAsync("MessageListener", chatMessageModel);
            } catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
            return Ok(chatMessageModel);
        }

        [AllowAnonymous]
        [Route("GetRecentChatUser")]
        [HttpGet]
        public async Task<IActionResult> GetRecentChatUser(string userId, long lastChatSessionId)
        {
            IEnumerable<ChatSession> recentChatSession = await _chatRepository.GetRecentChatSession(userId, lastChatSessionId);

            UserDisplayPagination userDisplayPagination = new UserDisplayPagination();
            List<UserDisplay> recentUser = new List<UserDisplay>();

            foreach (ChatSession chatSession in recentChatSession) {
                try {
                    CustomUser user = new CustomUser();
                    string picture = "";
                    
                    if (chatSession.FirstUserId == userId) {
                        user = await _userManager.Users.FirstAsync(e => e.Id == chatSession.SecondUserId);
                    } else if (chatSession.SecondUserId == userId) {
                        user = await _userManager.Users.FirstAsync(e => e.Id == chatSession.FirstUserId);
                    }
                    picture = await _picturesRepository.GetProfilePicture(user.Id);
                    
                    recentUser.Add(new UserDisplay{
                        UserId = user.Id,
                        Email = user.Email,
                        ChatSessionId = chatSession.Id,
                        UserName = user.UserName,
                        UserProfile = picture,
                        Role = "User"
                    });
                } catch (Exception) {
                    continue;
                }
            }

            userDisplayPagination.LastChatSessionId = recentChatSession.Last().Id;
            userDisplayPagination.Users = recentUser;

            return Ok(userDisplayPagination);
        }

        private ChatMessageModel CreateChatMessageModelObject(ChatMessages chatMessages)
        {
            ChatMessageModel chatMessageModel = new ChatMessageModel();

            chatMessageModel.Id = chatMessages.Id;
            chatMessageModel.SentUserId = chatMessages.SentUserId;
            chatMessageModel.ChatMessage = chatMessages.ChatMessage;
            chatMessageModel.CreatedDate = chatMessages.CreatedDate;
            chatMessageModel.IsRead = chatMessages.IsRead;

            return chatMessageModel;
        }

        [Authorize]
        [Route("GetRecentChatMessage")]
        [HttpGet]
        public async Task<IActionResult> GetRecentChatMessage(long chatSessionId, string lastMessageSentDate) 
        {   
            DateTime date = DateTime.Parse(lastMessageSentDate);
            List<ChatMessages> chats = await _chatRepository.GetRecentChatMessagesUser(chatSessionId, date);

            ChatMessageDisplayPagination messageDisplayPagination = new ChatMessageDisplayPagination();
            messageDisplayPagination.chats = new List<ChatMessageDisplay>();
            messageDisplayPagination.LastChatDate = chats.Last().CreatedDate;

            ChatMessageDisplay chatMessageDisplay = new ChatMessageDisplay();
            chatMessageDisplay.ListOfChatMessage = new List<ChatMessageModel>();

            for (int i = 0; i < chats.Count() - 1; i++) 
            {

                if (chats[i].CreatedDate.Day != chats[i + 1].CreatedDate.Day) 
                {
                    if (chatMessageDisplay.ListOfChatMessage.IsNullOrEmpty())
                    {
                        chatMessageDisplay.ChatDate = chats[i].CreatedDate;

                        chatMessageDisplay.ListOfChatMessage.Add(CreateChatMessageModelObject(chats[i]));
                    }

                    messageDisplayPagination.chats.Add(chatMessageDisplay);
                    
                    chatMessageDisplay = new ChatMessageDisplay();
                    chatMessageDisplay.ListOfChatMessage = new List<ChatMessageModel>();

                    continue;
                }

                int chatHour = chats[i + 1].CreatedDate.Hour - chats[i].CreatedDate.Hour;
                if (chatHour < 1) {
                    chatMessageDisplay.ChatDate = chats[i].CreatedDate;
                    if (chatMessageDisplay.ListOfChatMessage.IsNullOrEmpty())
                    {
                        chatMessageDisplay.ListOfChatMessage.Add(CreateChatMessageModelObject(chats[i]));
                        chatMessageDisplay.ListOfChatMessage.Add(CreateChatMessageModelObject(chats[i + 1]));
                    } else
                    {
                        chatMessageDisplay.ListOfChatMessage.Add(CreateChatMessageModelObject(chats[i + 1]));
                    }
                }
                else
                {
                    messageDisplayPagination.chats.Add(chatMessageDisplay);

                    chatMessageDisplay = new ChatMessageDisplay();
                    chatMessageDisplay.ListOfChatMessage = new List<ChatMessageModel>();
                }
            }
            messageDisplayPagination.chats.Add(chatMessageDisplay);

            return Ok(messageDisplayPagination);
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
        public async Task<IActionResult> SearchUsers(string searchWho, string whoSearch) {
            var users = await _userManager.Users.Where(user => user.UserName.Contains(searchWho) || user.Email.Contains(searchWho) || user.Id.Contains(searchWho)).Take(15).ToListAsync();
            // var users = await _userManager.Users.Where(user => user.UserName.Contains(searchValues)).ToListAsync();
            List<UserDisplay> userDisplays = new List<UserDisplay>();

            foreach (var user in users) {
                try {
                    bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    if (isAdmin || user.Id.Equals(whoSearch)) {
                        continue;
                    }
                    string picture = await _picturesRepository.GetProfilePicture(user.Id);
                    ChatSession chatSession = await _chatRepository.GetSessionByUserId(whoSearch, user.Id);
                    userDisplays.Add(new UserDisplay{
                                UserId = user.Id,
                                Email = user.Email,
                                ChatSessionId = chatSession == null ? 0 : chatSession.Id,
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
