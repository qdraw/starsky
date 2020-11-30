import { shallow } from 'enzyme';
import React from 'react';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem, newIFileIndexItemArray } from '../../../interfaces/IFileIndexItem';
import ItemTextListView from './item-text-list-view';

describe("ItemTextListView", () => {

  it("renders (without state component)", () => {
    shallow(<ItemTextListView fileIndexItems={newIFileIndexItemArray()} callback={() => { }} />)
  });

  it("renders undefined", () => {
    var content = shallow(<ItemTextListView fileIndexItems={undefined as any} callback={() => { }} />)
    expect(content.exists(".warning-box")).toBeTruthy();
  });

  it("list of 1 file item", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: false,
      }
    ] as IFileIndexItem[];
    var list = shallow(<ItemTextListView fileIndexItems={fileIndexItems} callback={() => { }} />);

    expect(list.find("ul li").text()).toBe(fileIndexItems[0].fileName);
  });

  it("list of 1 error item", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.ServerError,
        isDirectory: false,
      }
    ] as IFileIndexItem[];
    var list = shallow(<ItemTextListView fileIndexItems={fileIndexItems} callback={() => { }} />);

    expect(list.find("ul li em").text()).toBe("ServerError");
    expect(list.find("ul li").text()).toContain(fileIndexItems[0].fileName);
  });

  it("list of 1 directory item", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: true,
      }
    ] as IFileIndexItem[];

    var callback = jest.fn()
    var list = shallow(<ItemTextListView fileIndexItems={fileIndexItems} callback={callback} />);

    expect(list.find("ul li button").text()).toBe(fileIndexItems[0].fileName);
  });

  it("list of 1 directory item callback", () => {
    var fileIndexItems = [
      {
        filePath: "/test/image.jpg",
        fileName: "image.jpg",
        status: IExifStatus.Ok,
        isDirectory: true,
      }
    ] as IFileIndexItem[];

    var callback = jest.fn()
    var list = shallow(<ItemTextListView fileIndexItems={fileIndexItems} callback={callback} />);

    list.find("ul li button").simulate("click");

    expect(callback).toBeCalled();
    expect(callback).toBeCalledWith(fileIndexItems[0].filePath);
  });
});