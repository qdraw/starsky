import { globalHistory } from '@reach/router';
import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as useFetch from '../hooks/use-fetch';
import { IArchive } from '../interfaces/IArchive';
import { IConnectionDefault, newIConnectionDefault } from '../interfaces/IConnectionDefault';
import { IExifStatus } from '../interfaces/IExifStatus';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import MenuTrash from './menu-trash';
import * as Modal from './modal';

describe("MenuTrash", () => {

  it("renders", () => {
    shallow(<MenuTrash defaultText="" callback={() => { }} />)
  });

  describe("with Context", () => {

    let contextValues: any;

    beforeEach(() => {
      var state = {
        subPath: "/",
        fileIndexItems: [{ status: IExifStatus.Deleted, filePath: "/trashed/test1.jpg", fileName: "test1.jpg" }]
      } as IArchive;

      contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      })
    });

    it("open hamburger menu", () => {
      var component = mount(<MenuTrash />)
      var hamburger = component.find('.hamburger');

      expect(component.exists('.form-nav')).toBeTruthy();
      expect(component.exists('.hamburger.open')).toBeFalsy();
      expect(component.exists('.nav.open')).toBeFalsy();

      act(() => {
        hamburger.simulate('click');
      });

      expect(component.html()).toContain('hamburger open')
      expect(component.html()).toContain('nav open')
      expect(component.exists('.form-nav')).toBeTruthy();

      component.unmount();
    });

    it("select is not disabled", () => {
      var component = mount(<MenuTrash />)

      expect(component.exists('.item--select')).toBeTruthy();
      expect(component.exists('.item--more')).toBeTruthy();

      component.unmount();
    });

    it("select toggle", () => {

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      act(() => {
        globalHistory.navigate("/");
      });

      var component = mount(<MenuTrash />)

      var select = component.find('.item--select');

      act(() => {
        select.simulate('click');
      });

      expect(globalHistory.location.search).toBe("?select=")
      component.unmount();

    });

    it("more select all", () => {

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=");
      });

      var component = mount(<MenuTrash />)

      var more = component.find('.item--more');
      expect(more.exists('.disabled')).toBeFalsy()

      act(() => {
        more.find('.menu-option').simulate('click');
      });

      expect(globalHistory.location.search).toBe("?select=test1.jpg")

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("more undoSelection", () => {

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

      var component = mount(<MenuTrash />)

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

    it("more force delete", () => {

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })


      var modalSpy = jest.spyOn(Modal, 'default')
        .mockImplementationOnce(({ children }) => { return <>{children}</> });

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = mount(<MenuTrash />)

      var item = component.find('[data-test="delete"]');

      act(() => {
        item.simulate('click');
      });

      expect(modalSpy).toBeCalled();

      expect(globalHistory.location.search).toBe("?select=test1.jpg")

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });

    it("more restore-from-trash", async () => {

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, 'default').mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      }).mockImplementationOnce(() => {
        return newIConnectionDefault();
      })

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test1.jpg");
      });

      var component = mount(<MenuTrash />)

      var item = component.find('[data-test="restore-from-trash"]');

      // need to await here
      await act(async () => {
        await item.simulate('click');
      });

      expect(globalHistory.location.search).toBe("?select=");

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlReplaceApi(), "fieldName=tags&search=%21delete%21&f=%2Fundefined%2Ftest1.jpg");

      // cleanup
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/");
        component.unmount();
      });
    });


  });
});
