namespace JobSearcher.Account
{
    public static class UserHelper
    {
        public static async Task<UserInDatabase?> GetCurrentUserAsync(HttpContext context, IAccount account)
        {
            if (context.Items["CurrentUser"] is UserInDatabase user)
                return user;
      
            if (context.Items["UserId"] is not int userId)
                return null;

            user = await account.GetUserById(userId);
            context.Items["CurrentUser"] = user;
    
            return user;
        }
    }
}

