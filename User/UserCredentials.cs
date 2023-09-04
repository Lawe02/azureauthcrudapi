namespace webapi.User
{
    public class UserCredentials
    {
        public static List<UserModel> Users = new List<UserModel>()
        {
            new UserModel()
            {
                UserName = "richard_user",
                GivenName = "Richard",
                EmailAddress = "ric@user.com",
                Password = "user",
                SurName = "Chalk",
                Role = "User",
            },
            new UserModel()
            {
                UserName = "richard_admin",
                GivenName = "Richard",
                EmailAddress = "ric@admin.com",
                Password = "admin",
                SurName = "Chalk",
                Role = "Admin",
            }
        };
    }
}
