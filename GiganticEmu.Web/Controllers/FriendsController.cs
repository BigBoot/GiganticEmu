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
        private readonly ApplicationDatabase _database;

        public FriendsController(ILogger<FriendsController> logger, ApplicationDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromHeader(Name = "X-API-KEY")] string token)
        {
            var user = await _database.Users
                .Where(user => user.AuthToken == token)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new FriendsGetResponse(RequestResult.Unauthorized));
            }

            var friends = await _database.Friends
                .Where(friend => friend.UserId == user.Id)
                .Select(friend => new FriendsGetResponse.FriendData(
                    friend.FriendUser.UserName,
                    friend.Accepted,
                    friend.Accepted && friend.FriendUser.LastOnline.AddMinutes(5) >= DateTimeOffset.UtcNow,
                    CreateIconHash(friend.FriendUser.Email)
                ))
                .ToListAsync();

            var friendRequests = await _database.Friends
                .Where(friend => friend.FriendUserId == user.Id && friend.Accepted == false)
                .Select(friend => new FriendsGetResponse.FriendRequestData(friend.User.UserName, CreateIconHash(friend.User.Email)))
                .ToListAsync();

            user.LastOnline = DateTimeOffset.UtcNow;
            await _database.SaveChangesAsync();

            return Ok(new FriendsGetResponse(RequestResult.Success)
            {
                Friends = friends,
                FriendRequests = friendRequests
            });
        }

        [HttpPost("invite")]
        [Produces("application/json")]
        public async Task<IActionResult> PostInvite(FriendsInvitePostRequest request, [FromHeader(Name = "X-API-KEY")] string token)
        {
            var user = await _database.Users
                          .Where(user => user.AuthToken == token)
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new FriendsInvitePostResponse(RequestResult.Unauthorized));
            }

            var friend = await _database.Users
                          .Where(user => user.UserName == request.UserName)
                          .SingleOrDefaultAsync();

            if (friend == null || friend.Id == user.Id)
            {
                return Ok(new FriendsInvitePostResponse(RequestResult.InvalidUser));
            }

            bool Accepted = false;
            if (await _database.Friends.Where(e => e.FriendUserId == user.Id && e.UserId == friend.Id).FirstOrDefaultAsync() is Friend existing)
            {
                Accepted = true;
                existing.Accepted = true;
            }

            if (!await _database.Friends.Where(e => e.UserId == user.Id && e.FriendUserId == friend.Id).AnyAsync())
            {
                var invite = new Friend()
                {
                    UserId = user.Id,
                    FriendUserId = friend.Id,
                    Accepted = Accepted,
                };

                await _database.Friends.AddAsync(invite);
                await _database.SaveChangesAsync();
            }

            return Ok(new FriendsInvitePostResponse(RequestResult.Success));
        }

        [HttpDelete]
        [HttpDelete("invite")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(FriendsDeleteRequest request, [FromHeader(Name = "X-API-KEY")] string token)
        {
            var user = await _database.Users
                          .Where(user => user.AuthToken == token)
                          .SingleOrDefaultAsync();

            if (user == null)
            {
                return Ok(new FriendsDeleteResponse(RequestResult.Unauthorized));
            }

            var friend = await _database.Users
                          .Where(user => user.UserName == request.UserName)
                          .SingleOrDefaultAsync();

            if (friend == null || friend.Id == user.Id)
            {
                return Ok(new FriendsDeleteResponse(RequestResult.InvalidUser));
            }

            var friends = await _database.Friends
                .Where(e => (e.FriendUserId == user.Id && e.UserId == friend.Id) || (e.FriendUserId == friend.Id && e.UserId == user.Id))
                .ToListAsync();

            foreach (var f in friends)
            {
                _database.Friends.Remove(f);
            }

            await _database.SaveChangesAsync();

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
