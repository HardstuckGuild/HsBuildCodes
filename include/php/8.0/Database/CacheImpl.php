<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

interface ICache {
	public function Get(string $path, string $schemaVersion = 'latest') : object;
}

class CacheEntry {
	public \DateTimeImmutable $CacheTime;
	public object             $Response;

	public function __construct(\DateTimeImmutable $cacheTime, object $response) {
		$this->CacheTime = $cacheTime;
		$this->Response  = $response;
	}
}

class DefaultCacheImpl implements ICache {
	public const CACHE_SECONDS = 30 * 60;

	/** @var CacheEntry[] */
	private static array $_cache = [];
	public function Get(string $path, string $schemaVersion = 'latest') : object
	{
		$key = $path.$schemaVersion;

		$ret = null;
		$now = new \DateTimeImmutable();
		if(array_key_exists($key, DefaultCacheImpl::$_cache))
		{
			$entry = DefaultCacheImpl::$_cache[$key];
			if(date_diff($entry->CacheTime, $now)->s > DefaultCacheImpl::CACHE_SECONDS)
			{
				$ret = API::RequestJson($path, schemaVersion: $schemaVersion);
				$entry->CacheTime = $now;
				$entry->Response = $ret;
			}
			else
			{
				$ret = $entry->Response;
			}
		}
		else
		{
			$ret = API::RequestJson($path, schemaVersion: $schemaVersion);
			DefaultCacheImpl::$_cache[$key] = new CacheEntry($now, $ret);
		}
		return $ret;
	}
}