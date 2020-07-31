import { globalHistory } from '@reach/router';
import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as useFetch from '../../../hooks/use-fetch';
import { IArchive } from '../../../interfaces/IArchive';
import { IConnectionDefault, newIConnectionDefault } from '../../../interfaces/IConnectionDefault';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import * as FetchPost from '../../../shared/fetch-post';
import { UrlQuery } from '../../../shared/url-query';
import * as DropArea from '../../atoms/drop-area/drop-area';
import * as ModalArchiveMkdir from '../modal-archive-mkdir/modal-archive-mkdir';
import * as ModalArchiveRename from '../modal-archive-rename/modal-archive-rename';
import * as ModalDownload from '../modal-download/modal-download';
import MenuArchive from './menu-archive';

describe("MenuArchive", () => {

  it("renders", () => {
    shallow(<MenuArchive />)
  });

  describe("with Context", () => {

    beforeEach(() => {
      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

      jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })
        .mockImplementationOnce(() => { })
    });

    it("default menu", () => {

      globalHistory.navigate("/");

      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="hamburger"]')).toBeTruthy();
      expect(component.exists('.item--select')).toBeTruthy();
      expect(component.exists('.item--more')).toBeTruthy();

      // and clean
      component.unmount();
    });

    it("check if on click the hamburger opens", () => {
      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="hamburger"] .open')).toBeFalsy();

      component.find('[data-test="hamburger"]').simulate('click');
      expect(component.exists('[data-test="hamburger"] .open')).toBeTruthy();

      component.unmount();
    });

    it("none selected", () => {

      globalHistory.navigate("?select=");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="selected-0"]')).toBeTruthy();

      component.unmount();
    });

    it("two selected", () => {

      globalHistory.navigate("?select=test1,test2");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      expect(component.exists('[data-test="selected-2"]')).toBeTruthy();

      component.unmount();
    });


    it("menu click mkdir", () => {

      globalHistory.navigate("/");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      var mkdirModalSpy = jest.spyOn(ModalArchiveMkdir, 'default').mockImplementationOnce(() => {
        return <></>
      })

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      var item = component.find('[data-test="mkdir"]');

      act(() => {
        item.simulate('click');
      });

      expect(mkdirModalSpy).toBeCalled();

      component.unmount();

    });

    it("menu click rename(dir)", () => {

      globalHistory.navigate("/");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      var renameModalSpy = jest.spyOn(ModalArchiveRename, 'default').mockImplementationOnce(() => {
        return <></>
      })

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      var item = component.find('[data-test="rename"]');
      console.log(item.html());


      act(() => {
        item.simulate('click');
      });

      expect(renameModalSpy).toBeCalled();

      component.unmount();

    });

    it("more and click on select all", () => {
      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      var component = mount(<MenuArchive />);

      var more = component.find('.item--more');

      act(() => {
        more.find('.menu-option').first().simulate('click');
      });

      // you did press to de-select all
      expect(globalHistory.location.search).toBe("?select=")

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });


    it("more undoSelection", () => {
      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = mount(<MenuArchive />);

      var more = component.find('.item--more');
      act(() => {
        more.find('.menu-option').first().simulate('click');
      });

      expect(globalHistory.location.search).toBe("?select=")

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("menu click MessageMoveToTrash", () => {

      globalHistory.navigate("/?select=test1.jpg");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }


      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var connectionDefault: IConnectionDefault = {
        statusCode: 200,
        data: [{ fileName: 'test.jpg', parentDirectory: '/' }] as IFileIndexItem[]
      };
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(connectionDefault);
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var component = mount(<MenuArchive />);

      var item = component.find('[data-test="trash"]');

      act(() => {
        item.simulate('click');
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlUpdateApi(), 'f=%2Fundefined%2Ftest1.jpg&Tags=%21delete%21&append=true&Colorclass=8&collections=true');

      component.unmount();

    });

    it("menu click export", () => {

      globalHistory.navigate("/?select=test1.jpg");

      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var exportModalSpy = jest.spyOn(ModalDownload, 'default').mockImplementationOnce(() => {
        return <></>
      })

      var component = mount(<MenuArchive />);

      var item = component.find('[data-test="export"]');

      act(() => {
        item.simulate('click');
      });

      expect(exportModalSpy).toBeCalled();

      component.unmount();

    });

    it("readonly - menu click mkdir", () => {
      globalHistory.navigate("/");

      var state = {
        subPath: "/",
        isReadOnly: true,
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(ModalArchiveMkdir, 'default').mockClear();

      var mkdirModalSpy = jest.spyOn(ModalArchiveMkdir, 'default').mockImplementationOnce(() => {
        return <></>
      });

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      var item = component.find('[data-test="mkdir"]');

      act(() => {
        item.simulate('click');
      });

      expect(mkdirModalSpy).toBeCalledTimes(0);

      component.unmount();
    });

    it("readonly - upload", () => {
      globalHistory.navigate("/");

      var state = {
        subPath: "/",
        isReadOnly: true,
        fileIndexItems: [{ status: IExifStatus.Ok, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      var dropAreaSpy = jest.spyOn(DropArea, 'default').mockImplementationOnce(() => {
        return <></>
      });

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      var component = mount(<MenuArchive />);

      expect(dropAreaSpy).toBeCalledTimes(0);

      component.unmount();
    });

  });

});

