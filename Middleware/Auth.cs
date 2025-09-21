
using JobSearcher.Jwt;

namespace JobSearcher.Api.MiddleWare{
    public class JwtMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
        {
            this.next = next;
            this.configuration = configuration;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IJwtService jwtService)
        {
            try
            {
                var token = context.Request.Cookies["jwt_token"];

                if (token is not null) AttachUserToContext(context, token, jwtService);

                await next(context);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

        }

        public void AttachUserToContext(HttpContext context, string token, IJwtService jwtService)
        {
            try
            {
                var userId = jwtService.ValidateToken(token);
               
                if (userId != null)
                {
                    context.Items["UserId"] = userId;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error attaching user to context.");
            }
        }
    }

}