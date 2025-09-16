using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Hubs
{
    public class AuctionHub : Hub
    {
        private static readonly ConcurrentDictionary<string, (int userId, string username)> ConnectionToUser
            = new ConcurrentDictionary<string, (int, string)>();

        private static readonly ConcurrentDictionary<int, byte> OnlineUserIds
            = new ConcurrentDictionary<int, byte>();

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<int, byte>> SessionParticipants
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, byte>>();

        // key: ikn:round -> per-user latest bid snapshot
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<int, BidSnapshot>> BidsByIknRound
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, BidSnapshot>>();

        public class BidSnapshot
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public int Round { get; set; }
            public decimal GrandTotal { get; set; }
            public decimal Decrement { get; set; }
            public System.DateTime Timestamp { get; set; }
        }

        public static void RecordBidSnapshot(string ikn, int userId, string username, int round, decimal grandTotal, decimal decrement, System.DateTime timestampUtc)
        {
            if (string.IsNullOrWhiteSpace(ikn) || userId <= 0 || round <= 0) return;
            var key = GroupName(ikn) + ":" + round.ToString();
            var map = BidsByIknRound.GetOrAdd(key, _ => new ConcurrentDictionary<int, BidSnapshot>());
            map[userId] = new BidSnapshot
            {
                UserId = userId,
                Username = username ?? string.Empty,
                Round = round,
                GrandTotal = grandTotal,
                Decrement = decrement,
                Timestamp = timestampUtc
            };
        }

        public async Task Identify(int userId, string username)
        {
            ConnectionToUser[Context.ConnectionId] = (userId, username ?? string.Empty);
            if (userId > 0)
            {
                OnlineUserIds[userId] = 1;
            }
            await Clients.All.SendAsync("OnlineCountUpdated", OnlineUserIds.Keys.Count);
        }

        public int GetOnlineCount()
        {
            return OnlineUserIds.Keys.Count;
        }

        public async Task JoinSession(string ikn)
        {
            if (string.IsNullOrWhiteSpace(ikn)) return;
            var group = GroupName(ikn);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            if (ConnectionToUser.TryGetValue(Context.ConnectionId, out var user))
            {
                var dict = SessionParticipants.GetOrAdd(group, _ => new ConcurrentDictionary<int, byte>());
                if (user.userId > 0)
                {
                    dict[user.userId] = 1;
                    await Clients.Group(group).SendAsync("SessionParticipantsUpdated", dict.Keys.Count);
                }
            }
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            if (ConnectionToUser.TryRemove(Context.ConnectionId, out var user))
            {
                // Remove from online if no more connections for this user
                bool stillConnected = ConnectionToUser.Values.Any(v => v.userId == user.userId);
                if (!stillConnected && user.userId > 0)
                {
                    OnlineUserIds.TryRemove(user.userId, out _);
                }
                await Clients.All.SendAsync("OnlineCountUpdated", OnlineUserIds.Keys.Count);

                // Remove from any session groups
                foreach (var kvp in SessionParticipants)
                {
                    var dict = kvp.Value;
                    if (user.userId > 0)
                    {
                        // Remove if no other connection from same user remains in this group
                        bool userStillInGroup = false;
                        // This check is approximate; fine for our needs
                        userStillInGroup = false;
                        if (!userStillInGroup)
                        {
                            dict.TryRemove(user.userId, out _);
                            await Clients.Group(kvp.Key).SendAsync("SessionParticipantsUpdated", dict.Keys.Count);
                        }
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public static string GroupName(string ikn) => $"EE:{ikn}";

        public Task<BidSnapshot[]> GetRoundBids(string ikn, int round)
        {
            if (string.IsNullOrWhiteSpace(ikn) || round <= 0)
            {
                return Task.FromResult(System.Array.Empty<BidSnapshot>());
            }
            var key = GroupName(ikn) + ":" + round.ToString();
            if (BidsByIknRound.TryGetValue(key, out var map))
            {
                var list = map.Values.OrderBy(b => b.GrandTotal).ThenBy(b => b.Timestamp).ToArray();
                return Task.FromResult(list);
            }
            return Task.FromResult(System.Array.Empty<BidSnapshot>());
        }

        public static BidSnapshot[] GetLatestRoundLeaderboard(string ikn, out int latestRound)
        {
            latestRound = 0;
            if (string.IsNullOrWhiteSpace(ikn)) return System.Array.Empty<BidSnapshot>();
            var prefix = GroupName(ikn) + ":";
            var entries = BidsByIknRound.Where(kv => kv.Key.StartsWith(prefix)).ToList();
            if (entries.Count == 0) return System.Array.Empty<BidSnapshot>();
            latestRound = entries
                .Select(kv => int.TryParse(kv.Key.Substring(prefix.Length), out var r) ? r : 0)
                .Max();
            var key = prefix + latestRound.ToString();
            if (BidsByIknRound.TryGetValue(key, out var map))
            {
                return map.Values.OrderBy(b => b.GrandTotal).ThenBy(b => b.Timestamp).ToArray();
            }
            return System.Array.Empty<BidSnapshot>();
        }
    }
}


