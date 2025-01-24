using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using rupbes.Models;

namespace rupbes.Providers
{
    public class CustomRoleProvider : RoleProvider
    {
        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            string[] rol = new string[] { };
            
            using (Database db = new Database())
            {
                Users user = db.Users.FirstOrDefault(u => u.login == username);
                if(user != null)
                {
                    Models.Roles role = db.Roles.Find(user.id_role);
                    if (role != null)
                    {
                        rol = new string[] { role.role };
                    }
                }
            }
            return rol;
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            bool ret = false;

            using (Database db = new Database())
            {
                Users user = db.Users.FirstOrDefault(u => u.login == username);
                if (user != null)
                {
                    Models.Roles userRole = user.Roles;
                    if(userRole != null && userRole.role == roleName)
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}