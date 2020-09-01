import { DifferenceInDate } from './date';
import { UrlQuery } from './url-query';

const cacheName = `starsky`;

export const ClearFileListCache = async (locationSearch: string): Promise<boolean> => {
  // not supported in your browser
  if (!('caches' in window)) {
    return false;
  }

  var location = new UrlQuery().UrlQueryServerApi(locationSearch);

  var cacheStorage = await openCacheStorage();
  if (!cacheStorage) {
    return false;
  }
  if (!await cacheStorage.delete(location)) {
    return false;
  }

  await cleanOldCacheItems();
  return true;
};

const cleanOldCacheItems = async (): Promise<void> => {
  var cache = await openCacheStorage();
  if (!cache) {
    return;
  }
  var keys = await cache.keys();
  keys.forEach(async key => {
    if (!CheckIfOld(key)) {
      if (cache) {
        cache.delete(key)
      }
    }
  });
}


export const CheckIfOld = (cachedResponse: Response | Request) => {
  var date = cachedResponse.headers.get('date');
  if (!date) return false;
  var diff = DifferenceInDate(new Date(date).valueOf());
  var diffResult = diff < 2; // 2 minutes
  return diffResult;
}

const openCacheStorage = async (): Promise<Cache | null> => {
  let cacheStorage: Cache;
  try {
    // not allowed witin the http context
    cacheStorage = await caches.open(cacheName)
  } catch (error) {
    return null;
  }
  return cacheStorage;
}


// Get data from the cache.
export async function GetCachedData(url: string): Promise<any | false> {

  // not supported in your browser
  if (!('caches' in window)) {
    return false;
  }

  var cacheStorage = await openCacheStorage();
  if (!cacheStorage) {
    return false;
  }
  const cachedResponse = await cacheStorage.match(url);

  if (!cachedResponse || !cachedResponse.ok || !CheckIfOld(cachedResponse)) {
    return false;
  }
  return await cachedResponse.json();
}

