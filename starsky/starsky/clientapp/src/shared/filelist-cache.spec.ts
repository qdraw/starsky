import { IArchive, newIArchive, SortType } from "../interfaces/IArchive";
import { IDetailView, newDetailView, PageType } from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { IFileIndexItem, newIFileIndexItem } from "../interfaces/IFileIndexItem";
import { IUrl } from "../interfaces/IUrl";
import { FileListCache } from "./filelist-cache";

describe("FileListCache", () => {
  const fileListCache = new FileListCache();
  describe("Test", () => {
    it("input and check if any result", () => {
      fileListCache.CacheSet("?f=1", {} as unknown as IArchive);

      const result = sessionStorage.getItem(
        fileListCache.CacheKeyGenerator({
          f: "1",
          collections: true,
          colorClass: []
        })
      );

      const cacheGetter = fileListCache.CacheGet("?f=1");

      expect(cacheGetter).toBeTruthy();
      expect(result).toBeTruthy();
    });

    it("CacheCleanByPath remove item by name", () => {
      const detailView = newDetailView();

      fileListCache.CacheSetObject(
        { f: "/test/image.jpg" },
        {
          ...detailView,
          fileIndexItem: {
            ...newIFileIndexItem(),
            tags: "hi",
            fileCollectionName: "image",
            parentDirectory: "/test",
            fileName: "image.jpg"
          }
        }
      );
      // should not be null -->
      const result1 = fileListCache.CacheGetObject({ f: "/test/image.jpg" });
      expect(result1).not.toBeNull();

      // action >
      fileListCache.CacheCleanByPath("/test/image.jpg");

      const result2 = fileListCache.CacheGetObject({ f: "/test/image.jpg" });

      expect(result2).toBeNull();
    });

    it("should update parent item when exist (collections on)", () => {
      const detailView = newDetailView();
      detailView.fileIndexItem = newIFileIndexItem();
      detailView.fileIndexItem.parentDirectory = "/test";
      detailView.fileIndexItem.fileCollectionName = "image";
      detailView.fileIndexItem.fileName = "image.jpg";

      fileListCache.CacheSet("?f=/test/image.jpg", detailView);

      // set also a parent item
      fileListCache.CacheSet("?f=/test", {
        ...newIArchive(),
        pageType: PageType.Archive,
        fileIndexItems: [detailView.fileIndexItem]
      });

      // only for debugging
      for (let index = 0; index < Object.keys(sessionStorage).length; index++) {
        new FileListCache().ParseJson(sessionStorage.getItem(Object.keys(sessionStorage)[index]));
      }

      // and now update
      fileListCache.CacheSetObject(
        { f: "/test/image.jpg" },
        {
          ...detailView,
          fileIndexItem: {
            ...newIFileIndexItem(),
            tags: "hi",
            fileCollectionName: "image",
            parentDirectory: "/test",
            fileName: "image.jpg"
          }
        }
      );

      const cacheGetter = fileListCache.CacheGet("?f=/test") as IArchive;

      expect(cacheGetter).toBeTruthy();

      expect(cacheGetter.fileIndexItems[0].tags).toBe("hi");
    });

    it("should update parent item when exist (collections OFF)", () => {
      const detailView = newDetailView();
      detailView.fileIndexItem = newIFileIndexItem();
      detailView.fileIndexItem.parentDirectory = "/test_off";
      detailView.fileIndexItem.fileCollectionName = "image";
      detailView.fileIndexItem.fileName = "image.jpg";

      fileListCache.CacheSet("?f=/test_off/image.jpg&collections=false", detailView);

      // set also a parent item
      fileListCache.CacheSet("?f=/test_off&collections=false", {
        ...newIArchive(),
        pageType: PageType.Archive,
        fileIndexItems: [detailView.fileIndexItem]
      });

      if ((window as unknown as { debug: boolean }).debug) {
        // only for debugging
        for (let index = 0; index < Object.keys(sessionStorage).length; index++) {
          const item = new FileListCache().ParseJson(
            sessionStorage.getItem(Object.keys(sessionStorage)[index])
          );
          console.log(Object.keys(sessionStorage)[index]);
          console.log(item);
        }
      }

      // and now update
      fileListCache.CacheSetObject(
        { f: "/test_off/image.jpg", collections: false },
        {
          ...detailView,
          fileIndexItem: {
            ...newIFileIndexItem(),
            tags: "hi",
            fileCollectionName: "image",
            parentDirectory: "/test_off",
            fileName: "image.jpg"
          }
        }
      );

      const cacheGetter = fileListCache.CacheGet("?f=/test_off&collections=false") as IArchive;

      expect(cacheGetter).toBeTruthy();

      expect(cacheGetter.fileIndexItems[0].tags).toBe("hi");
    });

    it("should ignore non valid parent item", () => {
      const detailView = newDetailView();
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
      fileListCache.CacheSet("", {} as unknown as IArchive);

      const result = sessionStorage.getItem(
        fileListCache.CacheKeyGenerator({
          f: "/",
          collections: true,
          colorClass: []
        })
      );

      const cacheGetter = fileListCache.CacheGet("");

      expect(cacheGetter).toBeTruthy();
      expect(result).toBeTruthy();
    });

    it("default sort value should be fileName", () => {
      fileListCache.CacheSet("", {} as unknown as IArchive);

      const explicitSet = fileListCache.CacheKeyGenerator({
        sort: SortType.fileName
      });

      const implicitSet = fileListCache.CacheKeyGenerator({});
      expect(explicitSet === implicitSet).toBeTruthy();
    });

    it("sort value should be fileName NOT imageFormat", () => {
      fileListCache.CacheSet("", {} as unknown as IArchive);

      const imageFormatStorageKey = fileListCache.CacheKeyGenerator({
        sort: SortType.imageFormat
      });

      const fileNameStorageKey = fileListCache.CacheKeyGenerator({
        sort: SortType.fileName
      });

      expect(imageFormatStorageKey === fileNameStorageKey).toBeFalsy();
    });

    it("should not confuse 20241106_171136_DSC00389_e2.jpg and 20241106_171136_DSC00389.psd", () => {
      const urlObject: IUrl = {
        f: "/__REPLACE__",
        collections: false,
        colorClass: [],
        sort: undefined
      };

      const parentDirectory = "/test";

      const fileIndexItems: IFileIndexItem[] = [
        {
          filePath: "/test/20241106_171136_DSC00389_e2.jpg",
          fileName: "20241106_171136_DSC00389_e2.jpg",
          fileCollectionName: "20241106_171136_DSC00389_e2",
          fileHash: "",
          status: IExifStatus.Ok,
          isDirectory: false,
          parentDirectory
        },
        {
          filePath: "/test/20241106_171136_DSC00389.psd",
          fileName: "20241106_171136_DSC00389.psd",
          fileCollectionName: "20241106_171136_DSC00389",
          fileHash: "",
          status: IExifStatus.Ok,
          isDirectory: false,
          parentDirectory
        }
      ];

      const parentItem: IArchive = {
        subPath: "/test",
        fileIndexItems,
        pageType: PageType.Archive,
        breadcrumb: [],
        relativeObjects: {
          nextFilePath: "",
          prevFilePath: "",
          nextHash: "",
          prevHash: "",
          args: [""]
        },
        colorClassActiveList: [],
        colorClassUsage: [],
        collectionsCount: 0,
        isReadOnly: false,
        dateCache: Date.now()
      };

      const detailViewItem: IDetailView = {
        fileIndexItem: {
          filePath: "/test/20241106_171136_DSC00389_e2.jpg",
          fileCollectionName: "20241106_171136_DSC00389_e2",
          fileHash: "",
          fileName: "20241106_171136_DSC00389_e2.jpg",
          status: IExifStatus.Ok,
          isDirectory: false,
          parentDirectory
        },
        subPath: "",
        pageType: PageType.DetailView,
        breadcrumb: [],
        relativeObjects: {
          nextFilePath: "",
          prevFilePath: "",
          nextHash: "",
          prevHash: "",
          args: [""]
        },
        colorClassActiveList: [],
        isReadOnly: false,
        dateCache: Date.now()
      };

      // Set the parent item in the cache
      fileListCache.CacheSetObject({ ...urlObject, f: "/test" }, parentItem);

      // Update the detail view item in the cache
      fileListCache.CacheSetObject(
        { ...urlObject, f: "/test/20241106_171136_DSC00389_e2.jpg" },
        detailViewItem
      );

      // Retrieve the updated parent item from the cache
      const updatedParentItem = fileListCache.CacheGetObject({
        ...urlObject,
        f: parentDirectory
      }) as IArchive;

      // Ensure the correct item was updated
      const updatedItem = updatedParentItem.fileIndexItems.find(
        (item) => item.fileName === "20241106_171136_DSC00389_e2.jpg"
      );
      const notUpdatedItem = updatedParentItem.fileIndexItems.find(
        (item) => item.fileName === "20241106_171136_DSC00389.psd"
      );

      expect(updatedItem).toEqual(detailViewItem.fileIndexItem);
      expect(notUpdatedItem).toEqual(fileIndexItems[1]);
    });

    it("ignore when old", () => {
      sessionStorage.setItem(
        fileListCache.CacheKeyGenerator({
          f: "2",
          collections: true,
          colorClass: []
        }),
        JSON.stringify({
          data: 1,
          dateCache: 1
        } as { data: number; dateCache: number })
      );

      const cacheGetter = fileListCache.CacheGet("?f=2");

      expect(cacheGetter).toBeNull();
    });

    it("non valid json", () => {
      sessionStorage.setItem(
        fileListCache.CacheKeyGenerator({
          f: "2",
          collections: true,
          colorClass: []
        }),
        "non valid json"
      );

      console.error("--> should give non valid json");
      const cacheGetter = fileListCache.CacheGet("?f=2");

      expect(cacheGetter).toBeNull();

      sessionStorage.removeItem(
        fileListCache.CacheKeyGenerator({
          f: "2",
          collections: true,
          colorClass: []
        })
      );
    });

    it("should ignore other keys", () => {
      sessionStorage.setItem(
        "some_other_key",
        JSON.stringify({
          data: 1,
          dateCache: 1
        } as { data: number; dateCache: number })
      );

      fileListCache.CacheCleanOld();

      // should ignore other keys
      expect(sessionStorage.getItem("some_other_key")).toBeTruthy();
    });

    it("should ignore key Without dateCache", () => {
      const key = fileListCache.CacheKeyGenerator({
        f: "3",
        collections: true
      });
      sessionStorage.setItem(
        key,
        JSON.stringify({
          data: 2
        } as { data: number; dateCache: number })
      );
      fileListCache.CacheCleanOld();

      // its ignored by the getter
      const cacheGetter2 = fileListCache.CacheGet("?f=3");
      expect(cacheGetter2).toBeNull();

      // but the key does exist
      expect(sessionStorage.getItem(key)).toBeTruthy();
    });

    it("clean old items", () => {
      sessionStorage.setItem(
        fileListCache.CacheKeyGenerator({
          f: "3",
          collections: true,
          colorClass: []
        }),
        JSON.stringify({
          data: 2,
          dateCache: 2
        } as { data: number; dateCache: number })
      );

      const data4 = {
        data: 2,
        dateCache: Date.now()
      } as { data: number; dateCache: number };
      sessionStorage.setItem(
        fileListCache.CacheKeyGenerator({
          f: "4",
          collections: true,
          colorClass: []
        }),
        JSON.stringify(data4)
      );

      fileListCache.CacheCleanOld();

      const cacheGetter2 = fileListCache.CacheGet("?f=3");
      expect(cacheGetter2).toBeNull();

      const cacheGetter4 = fileListCache.CacheGet("?f=4");

      expect(cacheGetter4).toStrictEqual(data4);
    });

    it("clean everything", () => {
      sessionStorage.setItem(
        fileListCache.CacheKeyGenerator({
          f: "9",
          collections: true,
          colorClass: []
        }),
        JSON.stringify({
          data: 2,
          dateCache: 2
        } as { data: number; dateCache: number })
      );

      sessionStorage.setItem(
        fileListCache.CacheKeyGenerator({
          f: "10",
          collections: true,
          colorClass: []
        }),
        JSON.stringify({
          data: 2,
          dateCache: Date.now()
        } as { data: number; dateCache: number })
      );

      fileListCache.CacheCleanEverything();

      const cacheGetter9 = fileListCache.CacheGet("?f=9");
      expect(cacheGetter9).toBeNull();

      const cacheGetter10 = fileListCache.CacheGet("?f=10");
      expect(cacheGetter10).toBeNull();
    });

    it("setter ignore when feature toggle is off", () => {
      localStorage.setItem("clientCache", "false");

      const data6 = {
        data: 2,
        dateCache: Date.now()
      } as { data: number; dateCache: number };

      fileListCache.CacheSetObject({ f: "6" }, data6 as unknown as IDetailView);

      const checkIfItemNotExist = sessionStorage.getItem(
        fileListCache.CacheKeyGenerator({
          f: "6",
          collections: true,
          colorClass: []
        })
      );
      expect(checkIfItemNotExist).toBeNull();

      localStorage.removeItem("clientCache");
    });

    it("getter ignore when feature toggle is off", () => {
      const data7 = {
        data: 2,
        dateCache: Date.now()
      } as { data: number; dateCache: number };

      // before disabling
      fileListCache.CacheSetObject({ f: "7" }, data7 as unknown as IDetailView);

      localStorage.setItem("clientCache", "false");

      const checkIfItemNotExist = fileListCache.CacheGetObject({ f: "6" });
      expect(checkIfItemNotExist).toBeNull();

      localStorage.removeItem("clientCache");
    });
  });
});
