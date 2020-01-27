import { shallow } from "enzyme";
import React from 'react';
import { newIArchive } from '../interfaces/IArchive';
import Search from './search';

describe("Search", () => {
  it("renders", () => {
    shallow(<Search {...newIArchive()} />)
  });
});