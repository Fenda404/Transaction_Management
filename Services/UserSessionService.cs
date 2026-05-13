using System.Linq;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    public static class UserSessionService
    {
        public static bool IsAdmin(int roleID) => roleID == 1;
        public static bool IsUser(int roleID) => roleID == 2;

        public static string GetRoleName(int roleID)
        {
            switch (roleID)
            {
                case 1:
                    return "ADMIN";
                case 2:
                    return "USER";
                default:
                    return "Unknown";
            }
        }
    }
}
