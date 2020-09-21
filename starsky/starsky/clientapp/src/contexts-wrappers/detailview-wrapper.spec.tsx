import { mount, shallow } from 'enzyme';
import React from 'react';
import * as DetailView from '../containers/detailview';
import { IDetailView, newDetailView } from '../interfaces/IDetailView';
import { newIFileIndexItem } from '../interfaces/IFileIndexItem';
import DetailViewWrapper from './detailview-wrapper';


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

});