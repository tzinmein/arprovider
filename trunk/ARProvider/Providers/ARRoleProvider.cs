using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using ARProvider.Models;
using Castle.ActiveRecord;

namespace ARProvider.Providers
{
    public class ARRoleProvider : RoleProvider
    {
        private string _FileName;
        private string _ApplicationName;


        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "ARRoleProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "AR Role Provider");
            }

            // Base initialization
            base.Initialize(name, config);

            // Initialize properties
            _ApplicationName = "OIF";
            foreach (string key in config.Keys)
            {
                if (key.ToLower().Equals("applicationname"))
                    ApplicationName = config[key];
                else if (key.ToLower().Equals("filename"))
                    _FileName = config[key];
            }
        }

 

        #region Properties

        public override string ApplicationName
        {
            get
            {
                return _ApplicationName;
            }
            set
            {
                _ApplicationName = value;
            }
        }

        #endregion

        #region Methods

        public override void CreateRole(string roleName)
        {
            try
            {
                BTTRole role = new BTTRole();
                role.Name = roleName;
                role.CreatedDate = DateTime.UtcNow;
                role.ModifiedDate = DateTime.UtcNow;
                role.ModifiedBy = new Guid(ConfigurationManager.AppSettings["adminGuid"]);
                ActiveRecordMediator<BTTRole>.Save(role);
            }
            catch
            {
                throw;
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            try
            {
                BTTRole[] roles = ActiveRecordMediator<BTTRole>.FindAllByProperty("Name", roleName);

                if (roles.Length > 0)
                {
                    ActiveRecordMediator<BTTRole>.Delete(roles[0]);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool RoleExists(string roleName)
        {
            try
            {
                if (ActiveRecordMediator<BTTRole>.FindAllByProperty("Name", roleName).Length > 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                throw;
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            try
            {
                // Get the roles to be modified
                foreach (string roleName in roleNames)
                {
                    BTTRole[] roles = ActiveRecordMediator<BTTRole>.FindAllByProperty("Name", roleName);
                    if (roles.Length > 0)
                    {
                        foreach (string userName in usernames)
                        {
                            if (!UserInRole(userName, roles[0]))
                            {
                                BTTUser[] users = ActiveRecordMediator<BTTUser>.FindAllByProperty("UserName", userName);
                                if (users.Length > 0)
                                {
                                    roles[0].AssignedUsers.Add(users[0]);

                                }
                                else
                                {
                                    throw new Exception("User, " + userName + "not found.");
                                }
                            }
                        }
                        using (new TransactionScope())
                        {
                            ActiveRecordMediator<BTTRole>.SaveAndFlush(roles[0]);
                        }
                    }
                }

            }
            catch
            {
                throw;
            }
        }

        private bool UserInRole(string userName, BTTRole role)
        {
            bool userinrole = false;

            foreach (BTTUser user in role.AssignedUsers)
            {
                if (user.UserName == userName)
                {
                    userinrole = true;
                    break;
                }
            }
            return userinrole;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            try
            {
                // Get the roles to be modified
                foreach (string roleName in roleNames)
                {
                    BTTRole[] roles = ActiveRecordMediator<BTTRole>.FindAllByProperty("Name", roleName);
                    if (roles.Length > 0)
                    {
                        foreach (string userName in usernames)
                        {
                            if (UserInRole(userName, roles[0]))
                            {
                                BTTUser[] users = ActiveRecordMediator<BTTUser>.FindAllByProperty("UserName", userName);
                                if (users.Length > 0)
                                {
                                    roles[0].AssignedUsers.RemoveAt(UserIndexInRole(users[0].UserName, roles[0]));
                                }
                                else
                                {
                                    throw new Exception("User, " + userName + "not found.");
                                }
                            }
                        }
                        using (new TransactionScope())
                        {
                            ActiveRecordMediator<BTTRole>.SaveAndFlush(roles[0]);
                        }
                    }
                }

            }
            catch
            {
                throw;
            }
        }

        public int UserIndexInRole(string userName, BTTRole role)
        {
            int rtn = -1;
            for (int i = 0; i < role.AssignedUsers.Count; i++ )
            {
                if (((BTTUser)role.AssignedUsers[i]).UserName == userName)
                {
                    rtn = i;
                    break;
                }
            }
            return rtn;
        }

        public override string[] GetAllRoles()
        {
            try
            {
                
                BTTRole[] roles = ActiveRecordMediator<BTTRole>.FindAll();
                string[] rolenames = new string[roles.Length];
                for (int i = 0; i < roles.Length; i++ )
                {
                    rolenames[i] = roles[i].Name;
                }

                return rolenames;

            }
            catch
            {
                throw;
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            try
            {
                BTTUser user = (BTTUser)HttpContext.Current.Cache["user" + username];
                if (user == null)
                {
                    user = ActiveRecordMediator<BTTUser>.FindAllByProperty("UserName", username)[0];
                }
                string[] results = new string[user.Roles.Count];
                for (int i = 0; i < results.Length; i++)
                    results[i] = ((BTTRole)user.Roles[i]).Name;
                return results;
            }
            catch
            {
                throw;
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            try
            {
                BTTRole[] roles = ActiveRecordMediator<BTTRole>.FindAllByProperty("Name", roleName);

                string[] results = new string[roles[0].AssignedUsers.Count];
                for (int i = 0; i < results.Length; i++)
                    results[i] = ((BTTUser)roles[0].AssignedUsers[i]).UserName;
                return results;
            }
            catch
            {
                throw;
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            try
            {
                BTTRole[] roles = ActiveRecordMediator<BTTRole>.FindAllByProperty("Name", roleName);
                if (roles.Length > 0)
                {
                    foreach (BTTUser assignedUser in roles[0].AssignedUsers)
                    {
                        if (assignedUser.UserName == username)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    throw new ProviderException("Role does not exist!");
                }
            }
            catch
            {
                throw;
            }
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            try
            {
                List<string> results = new List<string>();
                Regex Expression = new Regex(usernameToMatch.Replace("%", @"\w*"));

                BTTRole[] roles = ActiveRecordMediator<BTTRole>.FindAllByProperty("Name", roleName);
                if (roles.Length > 0)
                {
                    foreach (BTTUser assignedUser in roles[0].AssignedUsers)
                    {
                        if (Expression.IsMatch(assignedUser.UserName))
                        {
                            results.Add(assignedUser.UserName);
                        }
                    }
                }
                else
                {
                    throw new ProviderException("Role does not exist!");
                }

                return results.ToArray();
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
