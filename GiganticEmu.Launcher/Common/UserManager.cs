using GiganticEmu.Shared;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher
{
    public class UserManager
    {
        public record LoginResult(UserData? User, ICollection<string> Errors);
        public record FriendsResult(ICollection<FriendData>? Friends, ICollection<string> Errors);
        public record GeneralResult(ICollection<string> Errors);

        private IBackendApi _api;
        private ApiTokenHandler _auth;

        public UserManager()
        {
            _api = Locator.Current.GetService<IBackendApi>()!;
            _auth = Locator.Current.GetService<ApiTokenHandler>()!;
        }

        public async Task<FriendsResult> GetFriends()
        {
            try
            {
                var result = await _api.GetFriends();
                if (result.Code == RequestResult.Success)
                {
                    return new FriendsResult(result.Friends.Select(friend => new FriendData()
                    {
                        UserName = friend.UserName,
                        IconHash = friend.IconHash,
                        Status = friend.Status,
                        HasAccepted = friend.Accepted,
                        CanAccept = false,
                        CanInvite = friend.CanInvite && !friend.CanJoin,
                        CanJoin = friend.CanJoin
                    }).Concat(result.FriendRequests.Select(request => new FriendData()
                    {
                        UserName = request.UserName,
                        IconHash = request.IconHash,
                        CanAccept = true,
                        HasAccepted = false,
                        CanJoin = false,
                        CanInvite = false,
                        Status = UserStatus.Unknown,
                    })).ToList(), new List<string>());
                }
                else
                {
                    return new FriendsResult(null, new List<string> { result.Message });
                }
            }
            catch (Exception ex)
            {
                return new FriendsResult(null, new List<string> { ex.Message });
            }
        }

        public async Task<GeneralResult> RemoveFriend(string userName)
        {
            try
            {
                var result = await _api.RemoveFriend(new FriendsDeleteRequest { UserName = userName });
                if (result.Code == RequestResult.Success)
                {
                    return new GeneralResult(new List<string>());
                }
                else
                {
                    return new GeneralResult(new List<string> { result.Message });
                }
            }
            catch (Exception ex)
            {
                return new GeneralResult(new List<string> { ex.Message });
            }
        }

        public async Task<GeneralResult> SendFriendRequest(string userName)
        {
            try
            {
                var result = await _api.SendFriendRequest(new FriendsRequestPostRequest { UserName = userName });
                if (result.Code == RequestResult.Success)
                {
                    return new GeneralResult(new List<string>());
                }
                else
                {
                    return new GeneralResult(new List<string> { result.Message });
                }
            }
            catch (Exception ex)
            {
                return new GeneralResult(new List<string> { ex.Message });
            }
        }

        public async Task<GeneralResult> InviteToGroup(string userName)
        {
            try
            {
                var result = await _api.SendInvite(new FriendsInvitePostRequest { UserName = userName });
                if (result.Code == RequestResult.Success)
                {
                    return new GeneralResult(new List<string>());
                }
                else
                {
                    return new GeneralResult(new List<string> { result.Message });
                }
            }
            catch (Exception ex)
            {
                return new GeneralResult(new List<string> { ex.Message });
            }
        }

        public async Task<LoginResult> Register(string username, string password, string email)
        {
            try
            {
                var result = await _api.Register(new UserPostRequest { UserName = username, Password = password, Email = email });
                if (result.Code == RequestResult.Success && result.AuthToken is string token)
                {
                    return await Login(token);
                }
                else
                {
                    return new LoginResult(null, result.Errors?.Select(err => $"{err.Message} [{err.Code}]").ToList() ?? new List<string> { result.Message });
                }
            }
            catch (Exception ex)
            {
                return new LoginResult(null, new List<string> { ex.Message });
            }
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            try
            {
                var result = await _api.Login(new SessionPostRequest { UserName = username, Password = password, RememberMe = true });
                if (result.Code == RequestResult.Success && result.AuthToken is string token)
                {
                    return await Login(token);
                }
                else
                {
                    return new LoginResult(null, new List<string> { result.Message });
                }
            }
            catch (Exception ex)
            {
                return new LoginResult(null, new List<string> { ex.Message });
            }
        }

        public async Task<LoginResult> Login(string authToken)
        {
            _auth.AuthToken = authToken;

            try
            {
                var result = await _api.GetSession();
                if (result.Code == RequestResult.Success && _auth.AuthToken is string token)
                {
                    return new LoginResult(new UserData
                    {
                        UserName = result.UserName!,
                        AuthToken = authToken
                    }, new List<string>());
                }
                else
                {
                    return new LoginResult(null, new List<string> { result.Message });
                }
            }
            catch (Exception ex)
            {
                return new LoginResult(null, new List<string> { ex.Message });
            }
        }
    }
}
