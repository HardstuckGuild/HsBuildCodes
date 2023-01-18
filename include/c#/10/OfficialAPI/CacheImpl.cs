using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Hardstuck.GuildWars2.BuildCodes.V2;

public interface ICache {
	public Task<T> Get<T>(string path, string schemaVersion = API.LATEST_SCHEMA);
}

class CacheEntry {
	public DateTime CacheTime;
	public object   Response = null!; // would just be `required` in c# 11
}

class DefaultCacheImpl : ICache {
	public static readonly TimeSpan CACHE_TIME = TimeSpan.FromMinutes(30);

	private static readonly Dictionary<string, CacheEntry> _cache = new();
	public async Task<T> Get<T>(string path, string schemaVersion = API.LATEST_SCHEMA)
	{
		var key = path+schemaVersion;

		T ret;
		var now = DateTime.Now;
		if(_cache.TryGetValue(key, out var entry))
		{
			if(now - entry.CacheTime > CACHE_TIME)
			{
				//NOTE(Rennorb): cant just put a ! behind because of ... i dont actaully know. it probalby just makes the task non-null because it binds stronger than await
				var ret_ = await API.RequestJson<T>(path, schemaVersion: schemaVersion);
				Debug.Assert(ret_ != null);
				entry.CacheTime = now;
				entry.Response  = ret = ret_;
			}
			else
			{
				ret = (T)entry.Response;
			}
		}
		else
		{
			//NOTE(Rennorb): cant just put a ! behind because of ... i dont actaully know. it probalby just makes the task non-null because it binds stronger than await
			var ret_ = await API.RequestJson<T>(path, schemaVersion: schemaVersion);
			Debug.Assert(ret_ != null);
			_cache.Add(key, new CacheEntry() {
				CacheTime = now,
				Response  = ret = ret_,
			});
		}

		return ret;
	}

	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Lifetime extension")]
	readonly Timer _cleanupTimer;
	public DefaultCacheImpl()
	{
		this._cleanupTimer = new(CleanDecayedObjects, null, TimeSpan.Zero, CACHE_TIME * 2);
	}

	/// <remarks> Gets called repeatedly by internal timer. May be called externally. </remarks>
	public void CleanDecayedObjects(object? _)
	{
		var now = DateTime.Now;
		foreach(var (key, entry) in _cache)
			if(now - entry.CacheTime > CACHE_TIME)
				_cache.Remove(key);
	}
}
