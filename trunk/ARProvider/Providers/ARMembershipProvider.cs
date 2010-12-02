using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using ARProvider.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;

namespace ARProvider.Providers
{
    public class ARMembershipProvider : MembershipProvider
    {
        private string _ApplicationName;
        private bool _EnablePasswordReset;
        private int _MaxInvalidPasswordAttempts;
        private int _MinRequiredNonAlphanumericChars;
        private int _MinRequiredPasswordLength;
        private string _Name;
        private MembershipPasswordFormat _PasswordFormat;
        private string _PasswordStrengthRegEx;
        private bool _RequiresQuestionAndAnswer;


        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "ARMembershipProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Active Record Membership Provider");
            }

            // Initialize the base class
            base.Initialize(name, config);

            // Initialize default values
            _ApplicationName = "OIF";
            _EnablePasswordReset = false;
            _PasswordStrengthRegEx = @"[\w| !§$%&/()=\-?\*]*";
            _MaxInvalidPasswordAttempts = 3;
            _MinRequiredNonAlphanumericChars = 1;
            _MinRequiredPasswordLength = 5;
            _RequiresQuestionAndAnswer = false;
            _PasswordFormat = MembershipPasswordFormat.Hashed;

            // Now go through the properties and initialize custom values
            foreach (string key in config.Keys)
            {
                switch (key.ToLower())
                {
                    case "name":
                        _Name = config[key];
                        break;
                    case "applicationname":
                        _ApplicationName = config[key];
                        break;
                    case "enablepasswordreset":
                        _EnablePasswordReset = bool.Parse(config[key]);
                        break;
                    case "passwordstrengthregex":
                        _PasswordStrengthRegEx = config[key];
                        break;
                    case "maxinvalidpasswordattempts":
                        _MaxInvalidPasswordAttempts = int.Parse(config[key]);
                        break;
                    case "minrequirednonalphanumericchars":
                        _MinRequiredNonAlphanumericChars = int.Parse(config[key]);
                        break;
                    case "minrequiredpasswordlength":
                        _MinRequiredPasswordLength = int.Parse(config[key]);
                        break;
                    case "passwordformat":
                        _PasswordFormat = (MembershipPasswordFormat) Enum.Parse(
                                                                         typeof (MembershipPasswordFormat), config[key]);
                        break;
                    case "requiresquestionandanswer":
                        _RequiresQuestionAndAnswer = bool.Parse(config[key]);
                        break;
                }
            }
        }

        #region Properties

        public override string ApplicationName
        {
            get { return _ApplicationName; }
            set { _ApplicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return _EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get
            {
                if (PasswordFormat == MembershipPasswordFormat.Hashed)
                    return false;
                else
                    return true;
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _MaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _MinRequiredNonAlphanumericChars; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _MinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return 20; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _PasswordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _PasswordStrengthRegEx; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }

        #endregion

        #region Methods

        public override MembershipUser CreateUser(string username, string password,
                                                  string email, string passwordQuestion,
                                                  string passwordAnswer, bool isApproved,
                                                  object providerUserKey, out MembershipCreateStatus status)
        {
            try
            {
                // Validate the username and email
                if (!ValidateUsername(username, email, Guid.Empty))
                {
                    status = MembershipCreateStatus.InvalidUserName;
                    return null;
                }

                // Raise the event before validating the password
                base.OnValidatingPassword(
                    new ValidatePasswordEventArgs(
                        username, password, true));

                // Validate the password
                if (!ValidatePassword(password))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }

                // Everything is valid, create the user
                var user = new BTTUser();

                user.UserName = username;
                user.PasswordSalt = string.Empty;
                user.Password = TransformPassword(password, ref user.PasswordSalt);
                user.Email = email;
                user.PasswordQuestion = passwordQuestion;
                user.PasswordAnswer = passwordAnswer;
                user.CreatedDate = DateTime.UtcNow;

                // Add the user to the store
                SaveUser(user);

                BTTUserActivity userActivity = GetActivityForUser(user);
                userActivity.FailedLogins = 0;
                userActivity.IsLockedOut = false;
                userActivity.LastActivityDate = DateTime.UtcNow;
                userActivity.LastPasswordChangeDate = DateTime.UtcNow;
                SaveUserActivity(userActivity);

                status = MembershipCreateStatus.Success;
                return CreateMembershipFromInternalUser(user);
            }
            catch
            {
                throw;
            }
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            try
            {
                BTTUser user = GetUser(username);
                if (user != null)
                {
                    ActiveRecordMediator<BTTUser>.Delete(user);
                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            try
            {
                BTTUser user = GetUser(username);
                if (user != null)
                {
                    if (userIsOnline)
                    {
                        BTTUserActivity uact = GetActivityForUser(user);
                        SaveUserActivity(uact);
                    }
                    return CreateMembershipFromInternalUser(user);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            try
            {
                BTTUser user = ActiveRecordMediator<BTTUser>.FindByPrimaryKey(providerUserKey);
                if (user != null)
                {
                    if (userIsOnline)
                    {
                        BTTUserActivity uact = GetActivityForUser(user);
                        SaveUserActivity(uact);
                    }
                    return CreateMembershipFromInternalUser(user);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            try
            {
                BTTUser[] users = ActiveRecordMediator<BTTUser>.FindAllByProperty("Email", email);
                if (users.Length > 0)
                {
                    return users[0].UserName;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            try
            {
                BTTUser aruser = ActiveRecordMediator<BTTUser>.FindByPrimaryKey(user.ProviderUserKey);

                if (aruser != null)
                {
                    if (!ValidateUsername(aruser.UserName, aruser.Email, aruser.GUID))
                        throw new ArgumentException("Username and / or email are not unique!");

                    aruser.Email = user.Email;
                    aruser.Comment = user.Comment;
                    SaveUser(aruser);
                }
                else
                {
                    throw new ProviderException("User does not exist!");
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            try
            {
                BTTUser user = GetUser(username);
                if (user == null)
                {
                    return false;
                }

                if (ValidateUserInternal(user, password))
                {
                    BTTUserActivity uact = GetActivityForUser(user);
                    uact.LastLoginDate = DateTime.UtcNow;
                    uact.LastActivityDate = DateTime.UtcNow;
                    uact.FailedLogins = 0;
                    SaveUserActivity(uact);

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

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                // Get the user from the store
                BTTUser user = GetUser(username);
                if (user == null)
                    throw new Exception("User does not exist!");

                if (ValidateUserInternal(user, oldPassword))
                {
                    // Raise the event before validating the password
                    base.OnValidatingPassword(
                        new ValidatePasswordEventArgs(
                            username, newPassword, false));

                    if (!ValidatePassword(newPassword))
                        throw new ArgumentException("Password doesn't meet password strength requirements!");

                    user.PasswordSalt = string.Empty;
                    user.Password = TransformPassword(newPassword, ref user.PasswordSalt);
                    SaveUser(user);

                    BTTUserActivity uact = GetActivityForUser(user);
                    uact.LastPasswordChangeDate = DateTime.UtcNow;
                    uact.LastLoginDate = DateTime.UtcNow;
                    uact.LastActivityDate = DateTime.UtcNow;
                    uact.FailedLogins = 0;
                    SaveUserActivity(uact);

                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password,
                                                             string newPasswordQuestion, string newPasswordAnswer)
        {
            try
            {
                // Get the user from the store
                BTTUser user = GetUser(username);

                if (ValidateUserInternal(user, password))
                {
                    user.PasswordQuestion = newPasswordQuestion;
                    user.PasswordAnswer = newPasswordAnswer;
                    SaveUser(user);


                    BTTUserActivity uact = GetActivityForUser(user);
                    uact.LastPasswordChangeDate = DateTime.UtcNow;
                    uact.LastLoginDate = DateTime.UtcNow;
                    uact.LastActivityDate = DateTime.UtcNow;
                    uact.FailedLogins = 0;
                    SaveUserActivity(uact);
                    return true;
                }

                return false;
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                                  out int totalRecords)
        {
            try
            {
                BTTUser[] users = ActiveRecordMediator<BTTUser>.FindAllByProperty("Email", emailToMatch);
                var usersl = new List<BTTUser>(users);


                totalRecords = users.Length;
                return CreateMembershipCollectionFromInternalList(usersl);
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
                                                                 out int totalRecords)
        {
            try
            {
                BTTUser[] users = ActiveRecordMediator<BTTUser>.FindAllByProperty("UserName", usernameToMatch);
                var usersl = new List<BTTUser>(users);

                totalRecords = usersl.Count;
                return CreateMembershipCollectionFromInternalList(usersl);
            }
            catch
            {
                throw;
            }
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                BTTUser[] users = ActiveRecordMediator<BTTUser>.FindAll();
                totalRecords = users.Length;
                var usersl = new List<BTTUser>(users);
                return CreateMembershipCollectionFromInternalList(usersl);
            }
            catch
            {
                throw;
            }
        }

        public override int GetNumberOfUsersOnline()
        {
            DateTime mustBeActiveAfter = DateTime.UtcNow.AddMinutes(-Membership.UserIsOnlineTimeWindow);

            var q = new SimpleQuery<BTTUserActivity>(
                @"from BTTUserActivity ua where ua.LastActivityDate > ?", mustBeActiveAfter);
            BTTUserActivity[] uas = q.Execute();

            return uas.Length;
        }

        public override string GetPassword(string username, string answer)
        {
            try
            {
                if (EnablePasswordRetrieval)
                {
                    BTTUser user = GetUser(username);

                    if (answer.Equals(user.PasswordAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        return user.Password;
                    }
                    else
                    {
                        throw new MembershipPasswordException();
                    }
                }
                else
                {
                    throw new Exception("Password retrieval is not enabled!");
                }
            }
            catch
            {
                throw;
            }
        }

        public override string ResetPassword(string username, string answer)
        {
            try
            {
                BTTUser user = GetUser(username);
                if (user.PasswordAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase))
                {
                    var NewPassword = new byte[16];
                    RandomNumberGenerator rng = RandomNumberGenerator.Create();
                    rng.GetBytes(NewPassword);

                    string NewPasswordString = Convert.ToBase64String(NewPassword);
                    user.PasswordSalt = string.Empty;
                    user.Password = TransformPassword(NewPasswordString, ref user.PasswordSalt);
                    ActiveRecordMediator<BTTUser>.Save(user);

                    return NewPasswordString;
                }
                else
                {
                    throw new Exception("Invalid answer entered!");
                }
            }
            catch
            {
                throw;
            }
        }

        public override bool UnlockUser(string userName)
        {
            BTTUser user = GetUser(userName);
            BTTUserActivity uact = GetActivityForUser(user);
            uact.IsLockedOut = false;
            uact.FailedLogins = 0;
            return true;
        }

        #endregion

        #region Private Helper Methods

        private string TransformPassword(string password, ref string salt)
        {
            string ret = string.Empty;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    ret = password;
                    break;

                case MembershipPasswordFormat.Hashed:

                    // Generate the salt if not passed in
                    if (string.IsNullOrEmpty(salt))
                    {
                        var saltBytes = new byte[16];
                        RandomNumberGenerator rng = RandomNumberGenerator.Create();
                        rng.GetBytes(saltBytes);
                        salt = Convert.ToBase64String(saltBytes);
                    }
                    ret = FormsAuthentication.HashPasswordForStoringInConfigFile(
                        (salt + password), "SHA1");
                    break;

                case MembershipPasswordFormat.Encrypted:
                    byte[] ClearText = Encoding.UTF8.GetBytes(password);
                    byte[] EncryptedText = base.EncryptPassword(ClearText);
                    ret = Convert.ToBase64String(EncryptedText);
                    break;
            }

            return ret;
        }

        private bool ValidateUsername(string userName, string email, Guid excludeKey)
        {
            if (ActiveRecordMediator<BTTUser>.FindAllByProperty("UserName", userName).Length > 0)
            {
                return false;
            }
            if (ActiveRecordMediator<BTTUser>.Exists(excludeKey))
            {
                return false;
            }
            if (ActiveRecordMediator<BTTUser>.FindAllByProperty("Email", email).Length > 0)
            {
                return false;
            }


            return true;
        }

        private bool ValidatePassword(string password)
        {
            bool IsValid = true;
            Regex HelpExpression;

            // Validate simple properties
            IsValid = IsValid && (password.Length >= MinRequiredPasswordLength);

            // Validate non-alphanumeric characters
            HelpExpression = new Regex(@"\W");
            IsValid = IsValid && (HelpExpression.Matches(password).Count >= MinRequiredNonAlphanumericCharacters);

            // Validate regular expression
            HelpExpression = new Regex(PasswordStrengthRegularExpression);
            IsValid = IsValid && (HelpExpression.Matches(password).Count > 0);

            return IsValid;
        }

        private bool ValidateUserInternal(BTTUser user, string password)
        {
            if (user != null)
            {
                BTTUserActivity uact = GetActivityForUser(user);

                if (uact.FailedLogins > _MaxInvalidPasswordAttempts || uact.IsLockedOut)
                {
                    return false;
                }
                string passwordValidate = TransformPassword(password, ref user.PasswordSalt);
                if (string.Compare(passwordValidate, user.Password) == 0)
                {
                    return true;
                }
                else
                {
                    uact.FailedLogins += 1;
                    SaveUserActivity(uact);
                }
            }

            return false;
        }

        private MembershipUser CreateMembershipFromInternalUser(BTTUser user)
        {
            BTTUserActivity uact = GetActivityForUser(user);

            var muser = new MembershipUser(base.Name,
                                           user.UserName, user.GUID, user.Email, user.PasswordQuestion,
                                           string.Empty, true, uact.IsLockedOut, user.CreatedDate.Value,
                                           uact.LastLoginDate,
                                           uact.LastActivityDate, uact.LastPasswordChangeDate, DateTime.MaxValue);

            return muser;
        }

        private MembershipUserCollection CreateMembershipCollectionFromInternalList(List<BTTUser> users)
        {
            var ReturnCollection = new MembershipUserCollection();

            foreach (BTTUser user in users)
            {
                ReturnCollection.Add(CreateMembershipFromInternalUser(user));
            }

            return ReturnCollection;
        }

        private BTTUserActivity GetActivityForUser(BTTUser user)
        {
            if (user == null)
                return null;

            BTTUserActivity BTTUserActivity = (BTTUserActivity) HttpContext.Current.Cache["activity" + user.UserName];
            if ( BTTUserActivity == null)
            {
                SimpleQuery<BTTUserActivity> q = new SimpleQuery<BTTUserActivity>(@"from BTTUserActivity ua where ua.AUser.GUID = ?", user.GUID);
                BTTUserActivity[] BTTUserActivities = q.Execute();
                if (BTTUserActivities.Length > 0)
                {
                    BTTUserActivity = BTTUserActivities[0];
                }
                else
                {
                    BTTUserActivity activity = new BTTUserActivity();
                    activity.CreatedDate = DateTime.UtcNow;
                    activity.AUser = user;
                    SaveUserActivity(activity);
                    BTTUserActivity = activity;
                }

                HttpContext.Current.Cache.Insert("activity" + user.UserName, BTTUserActivity, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(60));
            }

            return BTTUserActivity;
        }

        private static void SaveUserActivity(BTTUserActivity activity)
        {
            activity.ModifiedDate = DateTime.UtcNow;
            activity.LastActivityDate = DateTime.UtcNow;
            activity.ModifiedBy = new Guid(ConfigurationManager.AppSettings["adminGuid"]);
            ActiveRecordMediator<BTTUserActivity>.Save(activity);
        }

        private BTTUser GetUser(string username)
        {
            BTTUser user = (BTTUser)HttpContext.Current.Cache["user" + username];
            if (user == null)
            {
                BTTUser[] users = ActiveRecordMediator<BTTUser>.FindAllByProperty("UserName", username);
                if (users.Length > 0)
                {
                    user = users[0];
                    HttpContext.Current.Cache.Insert("user" + username, user, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(300));
                } 
            }

            
            return user;
        }

        private static void SaveUser (BTTUser user)
        {
            user.ModifiedDate = DateTime.UtcNow;
            user.ModifiedBy = new Guid(ConfigurationManager.AppSettings["adminGuid"]);
            ActiveRecordMediator<BTTUser>.Save(user);
        }
        #endregion
    }
}