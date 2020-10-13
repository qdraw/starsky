import { act } from '@testing-library/react';
import { mount, shallow } from 'enzyme';
import React from 'react';
import * as DetailView from '../containers/detailview';
import { useSocketsEventName } from '../hooks/realtime/use-sockets.const';
import { IDetailView, newDetailView } from '../interfaces/IDetailView';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import DetailViewWrapper, { DetailViewEventListenerUseEffect } from './detailview-wrapper';


describe("DetailViewWrapper", () => {

  it("renders", () => {
    shallow(<DetailViewWrapper {...newDetailView()} />)
  });

  describe("with mount", () => {
    it("check if DetailView is mounted", () => {
      var args = { ...newDetailView() } as IDetailView;
      var detailView = jest.spyOn(DetailView, 'default').mockImplementationOnce(() => { return <></> })

      mount(<DetailViewWrapper {...args} />);
      expect(detailView).toBeCalled();
    });

    it("check if dispatch is called", () => {
      var contextValues = { state: newIFileIndexItem(), dispatch: jest.fn() }
      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })

      var args = { ...newDetailView(), fileIndexItem: newIFileIndexItem() } as IDetailView;
      var detailView = jest.spyOn(DetailView, 'default').mockImplementationOnce(() => { return <></> })

      mount(<DetailViewWrapper {...args} />);

      expect(contextValues.dispatch).toBeCalled();
      expect(detailView).toBeCalled();
    });
  });

  describe("no context", () => {
    it("No context if used", () => {
      jest.spyOn(React, 'useContext').mockImplementationOnce(() => { return { state: null, dispatch: jest.fn() } })
      var args = { ...newDetailView() } as IDetailView;
      var compontent = mount(<DetailViewWrapper {...args} />);

      expect(compontent.text()).toBe('(DetailViewWrapper) = no state')
    });
  });

  describe("DetailViewEventListenerUseEffect", () => {

    const { location } = window;
    /**
     * Mock the location feature
     * @see: https://wildwolf.name/jest-how-to-mock-window-location-href/
     */
    beforeAll(() => {
      // @ts-ignore
      delete window.location;
      // eslint-disable-next-line @typescript-eslint/ban-ts-ignore
      // @ts-ignore
      window.location = {
        search: "/?f=/test.jpg",
      };
    });

    afterAll((): void => {
      window.location = location;
    });

    it("Check if event is received", (done) => {
      var dispatch = (e: any) => {
        // should ignore the first one
        expect(e).toStrictEqual(detail[1]);
        done();
      };

      function TestComponent() {
        DetailViewEventListenerUseEffect(dispatch);
        return (<></>)
      }

      var component = mount(<TestComponent />);

      var detail = [{
        ...newIFileIndexItem(),
        // should ignore this one
      },
      {
        "colorclass": undefined,
        ...newIFileIndexItem(),
        filePath: '/test.jpg',
        "type": "update",
      }];
      var event = new CustomEvent(useSocketsEventName, {
        detail
      });

      act(() => {
        document.body.dispatchEvent(event);
      });

      component.unmount();
    });
  });
});