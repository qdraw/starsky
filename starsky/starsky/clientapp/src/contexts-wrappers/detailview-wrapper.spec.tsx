import { mount, shallow } from 'enzyme';
import React from 'react';
import * as DetailView from '../containers/detailview';
import { IDetailView, newDetailView } from '../interfaces/IDetailView';
import DetailViewWrapper from './detailview-wrapper';


describe("DetailViewWrapper", () => {

  it("renders", () => {
    shallow(<DetailViewWrapper {...newDetailView()}></DetailViewWrapper>)
  });

  describe("with mount", () => {
    it("check if archive is mounted", () => {
      var args = { ...newDetailView() } as IDetailView;
      var archive = jest.spyOn(DetailView, 'default').mockImplementationOnce(() => { return <></> })

      mount(<DetailViewWrapper {...args}></DetailViewWrapper>);
      expect(archive).toBeCalled();
    });
  });

  describe("no context", () => {
    it("No context if used", () => {
      jest.spyOn(React, 'useContext').mockImplementationOnce(() => { return { state: null, dispatch: jest.fn() } })
      var args = { ...newDetailView() } as IDetailView;
      var compontent = mount(<DetailViewWrapper {...args}></DetailViewWrapper>);

      expect(compontent.text()).toBe('(DetailViewWrapper) => no state')
    });
  });

});