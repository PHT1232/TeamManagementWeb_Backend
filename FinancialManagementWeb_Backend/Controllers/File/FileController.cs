using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamManagementProject_Backend.Global;

namespace TeamManagementProject_Backend.Controllers.File 
{
    [AllowAnonymous]
    [Route("api/file/[action]")]
    public class FileController : ControllerBase {

        public FileController() {

        }

        [HttpGet]
        public IActionResult GetImage(string fileName) {
            string filePath = Path.Combine(AppFolders.UserProfilePictures + "/68ecba2d-a79a-4967-9637-0c75c8ad6c2e", fileName);

            Byte[] buffer = System.IO.File.ReadAllBytes(filePath);
            var file = File(buffer, "image/jpeg");
            return file;
        }
    
        [HttpGet]
        public IActionResult GetUserProfile(string user, string fileName) {
            string filePath = "";
            
            if (fileName == null || fileName == "undefined")
            {
                filePath = Path.Combine(AppFolders.UserDefaultProfilePictures + "/", "icons8-user-default-96.png");
            } else 
            {
                filePath = Path.Combine(AppFolders.UserProfilePictures + "/" + user, fileName);
            }

            byte[] buffer = System.IO.File.ReadAllBytes(filePath);
            var file = File(buffer, "image/jpeg");
            return file;
        }
    }
}