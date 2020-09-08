import { IArchive, newIArchive } from '../interfaces/IArchive';
import { newDetailView, PageType } from '../interfaces/IDetailView';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import { FileListCache } from './filelist-cache';

describe("FileListCache", () => {
  var fileListCache = new FileListCache();
  describe("Test", () => {

    it("input and check if any result", () => {
      fileListCache.CacheSet("?f=1", {} as any);

      var result = sessionStorage.getItem(fileListCache.CacheKeyGenerator({ f: '1', collections: true, colorClass: [] }));

      var cacheGetter = fileListCache.CacheGet("?f=1");

      expect(cacheGetter).toBeTruthy()
      expect(result).toBeTruthy()
    });

    it("should update parent item when exist (collections on)", () => {
      var detailView = newDetailView();
      detailView.fileIndexItem = newIFileIndexItem();
      detailView.fileIndexItem.parentDirectory = "/test";
      detailView.fileIndexItem.fileCollectionName = "image";
      detailView.fileIndexItem.fileName = "image.jpg";

      fileListCache.CacheSet("?f=/test/image.jpg", detailView);

      // set also a parent item
      fileListCache.CacheSet("?f=/test", { ...newIArchive(), pageType: PageType.Archive, fileIndexItems: [detailView.fileIndexItem] });

      // only for debugging
      for (let index = 0; index < Object.keys(sessionStorage).length; index++) {
        new FileListCache().ParseJson(sessionStorage.getItem(Object.keys(sessionStorage)[index]));
      }

      // and now update
      fileListCache.CacheSetObject({ f: '/test/image.jpg' }, {
        ...detailView, fileIndexItem: {
          ...newIFileIndexItem(), tags: 'hi', fileCollectionName: 'image', parentDirectory: "/test", fileName: 'image.jpg'
        },
      });

      var cacheGetter = fileListCache.CacheGet("?f=/test") as IArchive;

      expect(cacheGetter).toBeTruthy()

      expect(cacheGetter.fileIndexItems[0].tags).toBe('hi');
    });


    it("should update parent item when exist (collections OFF)", () => {
      var detailView = newDetailView();
      detailView.fileIndexItem = newIFileIndexItem();
      detailView.fileIndexItem.parentDirectory = "/test_off";
      detailView.fileIndexItem.fileCollectionName = "image";
      detailView.fileIndexItem.fileName = "image.jpg";

      fileListCache.CacheSet("?f=/test_off/image.jpg&collections=false", detailView);

      // set also a parent item
      fileListCache.CacheSet("?f=/test_off&collections=false", {
        ...newIArchive(),
        pageType: PageType.Archive, fileIndexItems: [detailView.fileIndexItem]
      });


      if ((window as any).debug) {
        // only for debugging
        for (let index = 0; index < Object.keys(sessionStorage).length; index++) {
          var item = new FileListCache().ParseJson(sessionStorage.getItem(Object.keys(sessionStorage)[index]));
          console.log(Object.keys(sessionStorage)[index]);
          console.log(item);
        }
      }

      // and now update
      fileListCache.CacheSetObject({ f: '/test_off/image.jpg', collections: false }, {
        ...detailView, fileIndexItem: {
          ...newIFileIndexItem(), tags: 'hi', fileCollectionName: 'image', parentDirectory: "/test_off", fileName: 'image.jpg'
        },
      });

      var cacheGetter = fileListCache.CacheGet("?f=/test_off&collections=false") as IArchive;

      expect(cacheGetter).toBeTruthy()

      expect(cacheGetter.fileIndexItems[0].tags).toBe('hi');
    });

    it("should ignore non valid parent item", () => {

      var detailView = newDetailView();
      detailView.fileIndexItem = newIFileIndexItem();
      detailView.fileIndexItem.parentDirectory = "/test_non_valid";
      detailView.fileIndexItem.fileCollectionName = "test_non_valid";
      detailView.fileIndexItem.fileName = "test_non_valid";
      // this should be an Archive not Detailview
      fileListCache.CacheSet("?f=/test_non_valid", detailView);

      // the detailview value needs to have a fileIndexItem value
      fileListCache.CacheSet("?f=/test_non_valid/image.jpg&collections=false", newDetailView());

      // expect nothing happend
    });



    it("check default value", () => {

      fileListCache.CacheSet("", {} as any);

      var result = sessionStorage.getItem(fileListCache.CacheKeyGenerator({ f: '/', collections: true, colorClass: [] }));

      var cacheGetter = fileListCache.CacheGet("");

      expect(cacheGetter).toBeTruthy()
      expect(result).toBeTruthy()
    });

    it("ignore when old", () => {

      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '2', collections: true, colorClass: [] }), JSON.stringify({
        data: 1,
        dateCache: 1,
      } as any));

      var cacheGetter = fileListCache.CacheGet("?f=2");

      expect(cacheGetter).toBeNull();
    });

    it("non valid json", () => {
      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '2', collections: true, colorClass: [] }), "non valid json");

      console.error('--> should give non valid json');
      var cacheGetter = fileListCache.CacheGet("?f=2");

      expect(cacheGetter).toBeNull();

      sessionStorage.removeItem(fileListCache.CacheKeyGenerator({ f: '2', collections: true, colorClass: [] }))
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
      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '3', collections: true, colorClass: [] }), JSON.stringify({
        data: 2,
        dateCache: 2,
      } as any));

      var data4 = {
        data: 2,
        dateCache: Date.now(),
      } as any;
      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '4', collections: true, colorClass: [] }), JSON.stringify(data4));

      fileListCache.CacheCleanOld();

      var cacheGetter2 = fileListCache.CacheGet("?f=3");
      expect(cacheGetter2).toBeNull();

      var cacheGetter4 = fileListCache.CacheGet("?f=4");

      expect(cacheGetter4).toStrictEqual(data4);
    });

    it("clean everything", () => {
      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '9', collections: true, colorClass: [] }), JSON.stringify({
        data: 2,
        dateCache: 2,
      } as any));

      sessionStorage.setItem(fileListCache.CacheKeyGenerator({ f: '10', collections: true, colorClass: [] }), JSON.stringify({
        data: 2,
        dateCache: Date.now(),
      } as any));

      fileListCache.CacheCleanEverything();

      var cacheGetter9 = fileListCache.CacheGet("?f=9");
      expect(cacheGetter9).toBeNull();

      var cacheGetter10 = fileListCache.CacheGet("?f=10");
      expect(cacheGetter10).toBeNull();
    });

    it("setter ignore when feature toggle is off", () => {
      localStorage.setItem('clientCache', 'false');

      var data6 = {
        data: 2,
        dateCache: Date.now(),
      } as any;

      fileListCache.CacheSetObject({ f: '6' }, data6);

      var checkIfItemNotExist = sessionStorage.getItem(fileListCache.CacheKeyGenerator({ f: '6', collections: true, colorClass: [] }));
      expect(checkIfItemNotExist).toBeNull();

      localStorage.removeItem('clientCache');
    });

    it("getter ignore when feature toggle is off", () => {

      var data7 = {
        data: 2,
        dateCache: Date.now(),
      } as any;

      // before disabling
      fileListCache.CacheSetObject({ f: '7' }, data7);

      localStorage.setItem('clientCache', 'false');

      var checkIfItemNotExist = fileListCache.CacheGetObject({ f: '6' });
      expect(checkIfItemNotExist).toBeNull();

      localStorage.removeItem('clientCache');
    });

  });
});
