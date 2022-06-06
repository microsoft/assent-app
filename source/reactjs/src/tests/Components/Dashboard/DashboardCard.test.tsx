import * as React from 'react';
import { render } from '@testing-library/react';
import { DashboardCard } from '../../../Components/Dashboard/DashboardCard'

describe("DashboardCard", () => {
    it("should DashboardCard renders correctly", () => {
      const wrapper = render(<DashboardCard />)
      expect(wrapper).toMatchSnapshot();
    });
});
