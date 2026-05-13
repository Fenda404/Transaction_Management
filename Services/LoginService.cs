using System.Linq;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    internal class LoginService
    {
        public bool Authenticate(string username, string password, out int roleID)
        {
            roleID = 0;
            using (var db = new TM_Database())
            {
                var user = db.Users
                             .FirstOrDefault(u => u.Username == username
                                               && u.PasswordHash == password);
                if (user == null) return false;

                roleID = (int)user.RoleID;
                UserSessionService.CurrentUser = user;
                return true;
            }
        }
    }
}
