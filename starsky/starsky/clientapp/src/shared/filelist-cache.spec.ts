import { FileListCache } from './filelist-cache';

describe("FileListCache", () => {
  var fileListCache = new FileListCache();
  describe("CacheSet", () => {


    it("input and check if any result", () => {

      fileListCache.CacheSet("?f=1", {});

      var result = sessionStorage.getItem(fileListCache.CacheKeyGenerator({ f: '1', collections: true }));

      var cacheGetter = fileListCache.CacheGet("?f=1");

      expect(cacheGetter).toBeTruthy()
      expect(result).toBeTruthy()
    });

    it("check default value", () => {

      fileListCache.CacheSet("", {});

      var result = sessionStorage.getItem(fileListCache.CacheKeyGenerator({ f: '/', collections: true }));

      var cacheGetter = fileListCache.CacheGet("");

      expect(cacheGetter).toBeTruthy()
      expect(result).toBeTruthy()
    });

    it("ignore when old", () => {

      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '2', collections: true }), JSON.stringify({
        data: 1,
        dateCache: 1,
      } as any));

      var cacheGetter = fileListCache.CacheGet("?f=2");

      expect(cacheGetter).toBeNull();
    });

    it("non valid json", () => {

      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '2', collections: true }), "non valid json");

      console.error('--> should give non valid json');
      var cacheGetter = fileListCache.CacheGet("?f=2");

      expect(cacheGetter).toBeNull();

      sessionStorage.removeItem(fileListCache.CacheKeyGenerator({ f: '2', collections: true }))
    });


    it("should ignore other keys", () => {

      sessionStorage.setItem("some_other_key", JSON.stringify({
        data: 1,
        dateCache: 1,
      } as any));

      fileListCache.CacheCleanOld();

      // should ignore other keys
      expect(sessionStorage.getItem("some_other_key")).toBeTruthy();

    });

    it("should ignore key Without dateCache", () => {

      var key = fileListCache.CacheKeyGenerator({ f: '3', collections: true });
      sessionStorage.setItem(key, JSON.stringify({
        data: 2,
      } as any));
      fileListCache.CacheCleanOld();

      // its ignored by the getter
      var cacheGetter2 = fileListCache.CacheGet("?f=3");
      expect(cacheGetter2).toBeNull();

      // but the key does exist
      expect(sessionStorage.getItem(key)).toBeTruthy();

    });


    it("clean old items", () => {
      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '3', collections: true }), JSON.stringify({
        data: 2,
        dateCache: 2,
      } as any));

      var data4 = {
        data: 2,
        dateCache: Date.now(),
      } as any;
      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '4', collections: true }), JSON.stringify(data4));

      fileListCache.CacheCleanOld();

      var cacheGetter2 = fileListCache.CacheGet("?f=3");
      expect(cacheGetter2).toBeNull();

      var cacheGetter4 = fileListCache.CacheGet("?f=4");

      expect(cacheGetter4).toStrictEqual(data4);
    });

  });
});
