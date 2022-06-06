import * as React from 'react';
import { render } from '@testing-library/react';
import {EmptyResults}  from '../../../../Components/Shared/Components/EmptyResults';

describe("EmptyResults", () => {
    it("should EmptyResults with message renders correctly", () => {
      const wrapper = render(<EmptyResults message={'testmessage'} />)
      expect(wrapper).toMatchSnapshot();
    });

    it("should EmptyResults without message renders correctly", () => {
      const wrapper = render(<EmptyResults />)
      expect(wrapper).toMatchSnapshot();
    });
});