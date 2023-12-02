using System;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RateLimit;
public class CustomRateLimiting : IRateLimiterPolicy<string>
{
   //public static readonly CustomRateLimiting Instance = new();
   bool _autoReplenishment;
   int _permitLimit;
   int _queueLimit;
   int _windowTimespan;

   public CustomRateLimiting(bool autoReplenishment,int permilLimit, int QueueLimit,int windowTimespan)
   {
       _autoReplenishment=autoReplenishment;
       _permitLimit=permilLimit;
       _queueLimit=QueueLimit;
       _windowTimespan=windowTimespan;
   }
   string userid="";
   public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        if  (string.IsNullOrEmpty(httpContext.Request.Headers["UserId"]))
        {
             Users user = new Users();
             user.UserId = Guid.NewGuid().ToString();
             httpContext.Request.Headers.Add("UserId",user.UserId);
             Repository resp=new Repository(ConnectDetails.conString);
             resp.SaveUserId(user,ConnectDetails.conString);
        }
       
        if (!string.IsNullOrEmpty(httpContext.Request.Headers["UserId"]))
        {
            Repository resp=new Repository(ConnectDetails.conString);
            userid=httpContext.Request.Headers["UserId"].ToString();
            if (!string.IsNullOrEmpty(resp.GetUserid(userid,ConnectDetails.conString)))
            {
                return RateLimitPartition.GetFixedWindowLimiter(userid,
                partition => new FixedWindowRateLimiterOptions()
                {
                    AutoReplenishment = _autoReplenishment,
                    PermitLimit = _permitLimit,
                    QueueLimit = _queueLimit,
                    Window = TimeSpan.FromMinutes(_windowTimespan)
                });
            }
            else{
                throw new Exception("Unautorised user");
            }
         }
        //    return RateLimitPartition.GetFixedWindowLimiter(userid,
        //         partition => new FixedWindowRateLimiterOptions()
        //         {
        //             AutoReplenishment = true,
        //             PermitLimit = 3,
        //             QueueLimit = 0,
        //             Window = TimeSpan.FromMinutes(1)
        //         });

        // return RateLimitPartition.GetFixedWindowLimiter(httpContext.Request.Headers.Host.ToString(),
        //     partition => new FixedWindowRateLimiterOptions
        //     {
        //         AutoReplenishment = true,
        //         PermitLimit = 3,
        //         QueueLimit = 0,
        //         Window = TimeSpan.FromMinutes(1)
        //     });
        return new RateLimitPartition<string>();
    }
   public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } =
        (context, _) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return new ValueTask();
        };
}
