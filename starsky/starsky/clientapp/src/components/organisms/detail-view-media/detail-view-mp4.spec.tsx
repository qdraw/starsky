import { mount, shallow } from 'enzyme';
import React from 'react';
import ReactDOM from 'react-dom';
import { IDetailView } from '../../../interfaces/IDetailView';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { IFileIndexItem } from '../../../interfaces/IFileIndexItem';
import * as Notification from '../../atoms/notification/notification';
import DetailViewMp4 from './detail-view-mp4';

describe("DetailViewMp4", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewMp4></DetailViewMp4>)
  });

  describe("with Context", () => {

    beforeEach(() => {
      jest.spyOn(HTMLMediaElement.prototype, 'load').mockImplementationOnce(() => {
        return Promise.resolve();
      })
    })

    it("click to play video", () => {
      var component = mount(<DetailViewMp4></DetailViewMp4>);

      var playSpy = jest.spyOn(HTMLMediaElement.prototype, 'play').mockImplementationOnce(() => {
        return Promise.resolve();
      })

      component.find('[data-test="video"]').simulate("click");

      expect(playSpy).toBeCalled();

      component.unmount();
    });

    it("click to play video and timeupdate", () => {
      var component = mount(<DetailViewMp4></DetailViewMp4>);

      var playSpy = jest.spyOn(HTMLMediaElement.prototype, 'play').mockImplementationOnce(() => {
        return Promise.resolve();
      })

      expect(component.find('.time').text()).toBe('');

      component.find('[data-test="video"]').simulate("click");

      expect(component.find('.time').text()).toBe('0:00 / 0:00');

      expect(playSpy).toBeCalled();

      component.unmount();
    });

    it("progress DOM", () => {

      const component = document.createElement('div');
      ReactDOM.render(<DetailViewMp4 />, component);
      var progress = component.querySelector('progress');
      if (progress == null) throw new Error('missing progress tag');
      progress.click();

    });

    it("progress ", () => {
      var component = mount(<DetailViewMp4></DetailViewMp4>);

      var playSpy = jest.spyOn(HTMLMediaElement.prototype, 'play').mockImplementationOnce(() => {
        return Promise.resolve();
      })

      Object.defineProperty(HTMLElement.prototype, 'offsetParent', {
        get() { return this.parentNode; },
      });

      var progress = component.find('progress').first().getDOMNode() as HTMLProgressElement;
      component.find('progress').simulate("click", { target: progress });

      expect(component.find('.time').text()).toBe('0:00 / 0:00');

      expect(playSpy).toBeCalled();

      component.unmount();
    });

    it("state not found and show error", () => {
      const state = {
        fileIndexItem: {
          status: IExifStatus.NotFoundSourceMissing
        } as IFileIndexItem
      } as IDetailView;

      var contextValues = { state, dispatch: jest.fn() }

      var useContextSpy = jest
        .spyOn(React, 'useContext')
        .mockImplementation(() => contextValues);


      var notificationSpy = jest.spyOn(Notification, 'default').mockImplementationOnce(() => {
        return <></>
      })
      var component = mount(<DetailViewMp4></DetailViewMp4>);

      expect(useContextSpy).toBeCalled();
      expect(notificationSpy).toBeCalled();

      component.unmount();
    });
  });
});