import * as React from 'react';
import { render } from '@testing-library/react';
import SuccessView from '../../../../../Components/Shared/Details/DetailsMessageBars/SuccessView';

describe('SuccessView', () => {
    it('should render Success view correctly', () => {
        const wrapper = render(<SuccessView />);
        expect(wrapper).toMatchSnapshot();
    });
});
