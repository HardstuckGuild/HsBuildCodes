import API from "./API";

export interface ICache {
	Get : (path : string , schemaVersion : string) => any;
}

class CacheEntry {
	public CacheTime : Date;
	public Response : any;

	public constructor(cacheTime : Date, response : any) {
		this.CacheTime = cacheTime;
		this.Response  = response;
	}
}

export class DefaultCacheImpl implements ICache {
	public static readonly CACHE_SECONDS = 30 * 60;

	private static _cache : Array<CacheEntry> = [];
	public async Get(path : string, schemaVersion : string = 'latest') : Promise<any>
	{
		const key = path+schemaVersion;

		let ret : any = null;
		const now = new Date();
		if(DefaultCacheImpl._cache[key] !== undefined)
		{
			const entry = DefaultCacheImpl._cache[key] as CacheEntry;
			if(now.getTime() - entry.CacheTime.getTime() > DefaultCacheImpl.CACHE_SECONDS)
			{
				ret = await API.RequestJson(path, undefined, schemaVersion);
				entry.CacheTime = now;
				entry.Response = ret;
			}
			else
			{
				ret = entry.Response;
			}
		}
		else
		{
			ret = await API.RequestJson(path, undefined, schemaVersion);
			DefaultCacheImpl._cache[key] = new CacheEntry(now, ret);
		}
		return ret;
	}
}