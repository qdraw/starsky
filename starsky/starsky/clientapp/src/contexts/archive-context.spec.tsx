import { IArchive, newIArchive } from '../interfaces/IArchive';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { IExifStatus } from '../interfaces/IExifStatus';
import { archiveReducer } from './archive-context';

describe("ArchiveContext", () => {

  it("remove - check if item is removed", () => {
    var state = {
      fileIndexItems: [{
        filePath: '/test.jpg'
      },
      {
        filePath: '/test1.jpg'
      },
      ]
    } as IArchiveProps;

    // fullpath input
    var action = { type: 'remove', toRemoveFileList: ['/test.jpg'] } as any

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe('/test1.jpg');
  });

  it("update - check if item is update (append false)", () => {
    var state = {
      ...newIArchive(),
      fileIndexItems: [{
        fileName: 'test.jpg'
      },
      {
        fileName: 'test1.jpg'
      },
      ],
      colorClassUsage: [] as number[],
    } as IArchive;
    var action = { type: 'update', tags: 'tags', colorclass: 1, description: 'description', title: 'title', append: false, select: ['test.jpg'] } as any

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);
    expect(result.fileIndexItems[0].tags).toBe('tags');
    expect(result.fileIndexItems[0].colorClass).toBe(1);
    expect(result.fileIndexItems[0].description).toBe('description');
    expect(result.fileIndexItems[0].title).toBe('title');
  });

  it("update - check if item is update (append false)", () => {
    var state = {
      fileIndexItems: [{
        fileName: 'test.jpg',
        tags: 'tags1',
        description: 'description1',
        title: 'title1'
      }
      ]
    } as IArchiveProps;
    var action = { type: 'update', tags: 'tags', description: 'description', title: 'title', append: true, select: ['test.jpg', 'notfound.jpg'] } as any

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].tags).toBe('tags1, tags');
    expect(result.fileIndexItems[0].description).toBe('description1description');
    expect(result.fileIndexItems[0].title).toBe('title1title');
  });

  it("add -- and check if is orderd", () => {
    // current state
    var state = {
      fileIndexItems: [{
        fileName: 'test0.jpg',
        filePath: '/test0.jpg',
        status: IExifStatus.Ok
      },
      {
        fileName: 'test2.jpg',
        filePath: '/test2.jpg',
        status: IExifStatus.Ok
      },
      ]
    } as IArchiveProps;

    // to add this
    var add = [{
      fileName: 'test1.jpg',
      filePath: '/test1.jpg',
      status: IExifStatus.Ok
    },
    {
      fileName: 'test3.jpg',
      filePath: '/test3.jpg',
      status: IExifStatus.Ok
    },
    ];
    var action = { type: 'add', add } as any
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(4);
    expect(result.fileIndexItems).toStrictEqual([
      {
        "fileName": "test0.jpg",
        "filePath": "/test0.jpg",
        status: IExifStatus.Ok
      },
      {
        "fileName": "test1.jpg",
        "filePath": "/test1.jpg",
        status: IExifStatus.Ok
      },
      {
        "fileName": "test2.jpg",
        "filePath": "/test2.jpg",
        status: IExifStatus.Ok
      },
      {
        "fileName": "test3.jpg",
        "filePath": "/test3.jpg",
        status: IExifStatus.Ok
      }]);
  });

  it("add -- and check if is orderd", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: '2018.01.01.17.00.01.jpg',
          filePath: '/__starsky/01-dif/2018.01.01.17.00.01.jpg',
          status: IExifStatus.Ok
        },
      ]
    } as IArchiveProps;

    // to add this
    var add = [{
      fileName: '__20180101170001.jpg',
      filePath: '/__starsky/01-dif/__20180101170001.jpg',
      status: IExifStatus.Ok
    },

    ];
    var action = { type: 'add', add } as any
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(2);

    expect(result.fileIndexItems[0].fileName).toBe('__20180101170001.jpg');

    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: '__20180101170001.jpg',
        filePath: '/__starsky/01-dif/__20180101170001.jpg',
        status: IExifStatus.Ok
      },
      {
        fileName: '2018.01.01.17.00.01.jpg',
        filePath: '/__starsky/01-dif/2018.01.01.17.00.01.jpg',
        status: IExifStatus.Ok
      }]);
  });

  it("add -- duplicate", () => {
    // current state
    var state = {
      fileIndexItems: [
        {
          fileName: '2018.01.01.17.00.01.jpg',
          filePath: '/__starsky/01-dif/2018.01.01.17.00.01.jpg',
          status: IExifStatus.Ok
        },
      ]
    } as IArchiveProps;

    // to add this
    var add = [{
      fileName: '2018.01.01.17.00.01.jpg',
      filePath: '/__starsky/01-dif/2018.01.01.17.00.01.jpg',
      status: IExifStatus.Ok,
      tags: 'updated'
    },

    ];
    var action = { type: 'add', add } as any
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);

    expect(result.fileIndexItems[0].fileName).toBe('2018.01.01.17.00.01.jpg');

    expect(result.fileIndexItems).toStrictEqual([
      {
        fileName: '2018.01.01.17.00.01.jpg',
        filePath: '/__starsky/01-dif/2018.01.01.17.00.01.jpg',
        status: IExifStatus.Ok,
        tags: 'updated'
      }]);
  });

  it("add -- duplicate", () => {

    // current state
    var state = {
      fileIndexItems: [{
        fileName: 'test0.jpg',
        filePath: '/test0.jpg',
        status: IExifStatus.Ok
      }
      ]
    } as IArchiveProps;

    var action = { type: 'add', add: state.fileIndexItems[0] } as any
    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);

  });

});