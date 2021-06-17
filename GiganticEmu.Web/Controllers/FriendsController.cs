using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly ILogger<FriendsController> _logger;
        private readonly IDbContextFactory<ApplicationDatabase> _databaseFactory;

        public FriendsController(ILogger<FriendsController> logger, IDbContextFactory<ApplicationDatabase> databaseFactory)
        {
            _logger = logger;
            _databaseFactory = databaseFactory;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromHeader(Name = "X-API-KEY")] string token)
        {
            var db = _databaseFactory.CreateDbContext();

            var user = await db.Users
                .Where(user => user.AuthToken == token)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new FriendsGetResponse(RequestResult.Unauthorized));
            }

            var friendRequests = await db.Friends
                .Where(friend => friend.FriendUserId == user.Id && friend.Accepted == false)
                .Select(friend => new FriendsGetResponse.FriendRequestData(friend.User.UserName, CreateIconHash(friend.User.Email)))
                .ToListAsync();

            var invites = await db.GroupInvites
                .Where(invite => invite.InvitedUserId == user.Id)
                .Select(invite => invite.User.UserName)
                .ToListAsync();

            var canInvite = user.SessionId != null && !user.InQueue;
            var friends = await db.Friends
                .Where(friend => friend.UserId == user.Id)
                .Include(friend => friend.FriendUser)
                .Select(friend => new FriendsGetResponse.FriendData(
                    friend.FriendUser.UserName,
                    friend.Accepted,
                    friend.Status,
                    CreateIconHash(friend.FriendUser.Email),
                    canInvite,
                    invites.Contains(friend.FriendUser.UserName)
                ))
                .ToListAsync();


            user.LastOnline = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Ok(new FriendsGetResponse(RequestResult.Success)
            {
                Friends = friends,
                FriendRequests = friendRequests,
            });
        }

        [HttpPost("request")]
        [Produces("application/json")]
        public async Task<IActionResult> PostRequest(FriendsInvitePostRequest request, [FromHeader(Name = "X-API-KEY")] string token)
        {
            var db = _databaseFactory.CreateDbContext();

            var user = await db.Users
                          .Where(user => user.AuthToken == token)
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new FriendsInvitePostResponse(RequestResult.Unauthorized));
            }

            var friend = await db.Users
                          .Where(user => user.UserName == request.UserName)
                          .SingleOrDefaultAsync();

            if (friend == null || friend.Id == user.Id)
            {
                return Ok(new FriendsInvitePostResponse(RequestResult.InvalidUser));
            }

            bool Accepted = false;
            if (await db.Friends.Where(e => e.FriendUserId == user.Id && e.UserId == friend.Id).FirstOrDefaultAsync() is Friend existing)
            {
                Accepted = true;
                existing.Accepted = true;
            }

            if (!await db.Friends.Where(e => e.UserId == user.Id && e.FriendUserId == friend.Id).AnyAsync())
            {
                var invite = new Friend()
                {
                    UserId = user.Id,
                    FriendUserId = friend.Id,
                    Accepted = Accepted,
                };

                await db.Friends.AddAsync(invite);
                await db.SaveChangesAsync();
            }

            return Ok(new FriendsInvitePostResponse(RequestResult.Success));
        }

        [HttpPost("invite")]
        [Produces("application/json")]
        public async Task<IActionResult> PostInvite(FriendsInvitePostRequest request, [FromHeader(Name = "X-API-KEY")] string token)
        {
            var db = _databaseFactory.CreateDbContext();

            var user = await db.Users
                          .Where(user => user.AuthToken == token)
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new FriendsInvitePostResponse(RequestResult.Unauthorized));
            }

            var invited = await db.Users
                          .Where(user => user.UserName == request.UserName)
                          .SingleOrDefaultAsync();

            if (invited == null || invited.Id == user.Id)
            {
                return Ok(new FriendsInvitePostResponse(RequestResult.InvalidUser));
            }

            if (await db.GroupInvites.Where(e => e.InvitedUserId == user.Id && e.UserId == invited.Id).FirstOrDefaultAsync() is GroupInvite existing)
            {
                var session = invited.SessionId;

                db.Remove(existing);
                await db.SaveChangesAsync();

                if (session == null || invited.InQueue)
                {
                    return Ok(new FriendsInvitePostResponse(RequestResult.SessionInvalid));
                }

                if (await db.Users.Where(user => user.SessionId == session).CountAsync() >= 5)
                {
                    return Ok(new FriendsInvitePostResponse(RequestResult.SessionFull));
                }

                user.ClearSession(false);
                user.SessionId = session;

                invited.SessionVersion++;

                await db.SaveChangesAsync();
            }
            else if (!await db.GroupInvites.Where(e => e.UserId == user.Id && e.InvitedUserId == invited.Id).AnyAsync())
            {
                var invite = new GroupInvite()
                {
                    UserId = user.Id,
                    InvitedUserId = invited.Id,
                };

                await db.GroupInvites.AddAsync(invite);
                await db.SaveChangesAsync();
            }

            return Ok(new FriendsInvitePostResponse(RequestResult.Success));
        }

        [HttpDelete]
        [HttpDelete("invite")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(FriendsDeleteRequest request, [FromHeader(Name = "X-API-KEY")] string token)
        {
            var db = _databaseFactory.CreateDbContext();

            var user = await db.Users
                          .Where(user => user.AuthToken == token)
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new FriendsDeleteResponse(RequestResult.Unauthorized));
            }

            var friend = await db.Users
                          .Where(user => user.UserName == request.UserName)
                          .SingleOrDefaultAsync();

            if (friend == null || friend.Id == user.Id)
            {
                return Ok(new FriendsDeleteResponse(RequestResult.InvalidUser));
            }

            var friends = await db.Friends
                .Where(e => (e.FriendUserId == user.Id && e.UserId == friend.Id) || (e.FriendUserId == friend.Id && e.UserId == user.Id))
                .ToListAsync();

            foreach (var f in friends)
            {
                db.Friends.Remove(f);
            }

            await db.SaveChangesAsync();

            return Ok(new FriendsDeleteResponse(RequestResult.Success));
        }

        private static string CreateIconHash(string email)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(email.Trim().ToLower()));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
