import { shallow } from "enzyme";
import React from 'react';
import { newDetailView } from '../interfaces/IDetailView';
import DetailView from './detailview';

describe("DetailView", () => {
  it("renders", () => {
    shallow(<DetailView {...newDetailView()} />)
  });

});