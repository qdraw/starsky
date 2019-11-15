import { shallow } from "enzyme";
import React from 'react';
import { newIArchive } from '../interfaces/IArchive';
import Archive from './archive';

describe("Archive", () => {
  it("renders", () => {
    shallow(<Archive {...newIArchive()} />)
  });
});