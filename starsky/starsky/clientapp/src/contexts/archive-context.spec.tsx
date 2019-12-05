import { IArchiveProps } from '../interfaces/IArchiveProps';
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
    var action = { type: 'remove', filesList: ['/test.jpg'] } as any

    var result = archiveReducer(state, action);

    expect(result.fileIndexItems.length).toBe(1);
    expect(result.fileIndexItems[0].filePath).toBe('/test1.jpg');
  });

  it("update - check if item is update (append false)", () => {
    var state = {
      fileIndexItems: [{
        fileName: 'test.jpg'
      },
      {
        fileName: 'test1.jpg'
      },
      ]
    } as IArchiveProps;
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

});