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

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            using (var db = new TM_Database())
            {

                if (db.Users.Any(u => u.Username == username))
                {
                    return false;
                }

                var newUser = new Users
                {
                    Username = username,
                    PasswordHash = password,
                    RoleID = 2
                };
                db.Users.Add(newUser);
                db.SaveChanges();
                return true;
            }
        }
    }
}
