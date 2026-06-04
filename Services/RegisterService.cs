using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Management.Models;

namespace Transaction_Management.Services
{
    internal class RegisterService
    {
        public bool Register(string username, string password)
        {
            using (var db = new TMDatabase())
            {
                if (db.Users.Any(u => u.Username == username))
                    return false;
                var newUser = new Users
                {
                    Username = username,
                    PasswordHash = password,
                    CreatedAt = DateTime.Now,
                    RoleID = 2,
                    Currency = "VNĐ",
                };
                db.Users.Add(newUser);
                db.SaveChanges();
                return true;
            }
        }
    }
}
