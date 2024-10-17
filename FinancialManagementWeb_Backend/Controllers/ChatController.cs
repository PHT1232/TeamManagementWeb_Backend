using EntityFramework.DbEntities.Chats;
using EntityFramework.Repository;
using EntityFramework.Repository.Chats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjectModel.ChatModels;
using TeamManagementProject_Backend.Controllers.Hubs;

namespace TeamManagementProject_Backend.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatRepository chatRepository
            , IHubContext<ChatHub> hubContext
            , UserManager<IdentityUser> userManager)
        {
            _chatRepository = chatRepository;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        // [Authorize]
        // [Route("GetAll")]
        // [HttpGet]
        // public async Task<IActionResult> GetAllChat()
        // {
        //     IEnumerable<Chat> chats = await _chatRepository.GetAll();
        //     return Ok(chats);
        // }

        [Authorize]
        [Route("Get")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new { Message = "Request Completed" });
        }

        [Authorize]
        [Route("SendMessage")]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageInsertModel chatModel)
        {
            if (chatModel == null)
            {
                throw new ("Chat is this real ?");
            }
            var user = await _userManager.FindByNameAsync(chatModel.SentId);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(chatModel.SentId);
            }

            ChatSession firstUserChatSession = new ChatSession {
                FirstUserId = chatModel.SentId,
                SecondUserId = chatModel.ReceivedId,
                CreatedDate = DateTime.Now,
            };

            ChatSession secondUserChatSession = new ChatSession {
                FirstUserId = chatModel.ReceivedId,
                SecondUserId = chatModel.SentId,
                CreatedDate = DateTime.Now,             
            };

            ChatMessages chatMessage = new ChatMessages {

            };

            var list = await _chatRepository.GetAll();
            await _hubContext.Clients.User(chatModel.SentId).SendAsync("TransferChartData", list);
            return Ok();
        }
    }
}
