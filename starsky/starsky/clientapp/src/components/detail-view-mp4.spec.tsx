import { mount, shallow } from 'enzyme';
import React from 'react';
import ReactDOM from 'react-dom';
import DetailViewMp4 from './detail-view-mp4';

describe("DetailViewGpx", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewMp4></DetailViewMp4>)
  });

  describe("with Context", () => {
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

    xit("progress DOM", () => {

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

    // it("timeupdate", () => {

    //   var playSpy = jest.spyOn(HTMLMediaElement.prototype, 'play').mockImplementationOnce(() => {
    //     return Promise.resolve();
    //   })


    //   const component = document.createElement('div');
    //   ReactDOM.render(<DetailViewMp4 />, component);


    //   var video = component.querySelector('video');
    //   if (video == null) throw new Error('missing video tag');

    //   console.log(video.play());

    //   video.currentTime = 50;

    //   video.dispatchEvent(new CustomEvent('timeupdate1', {
    //     bubbles: true,
    //   }));

    //   video.dispatchEvent(new Event('timeupdate1', { bubbles: true }));
    //   video.dispatchEvent(new CustomEvent('timeupdate1', { bubbles: true }))

    // });


  });
});