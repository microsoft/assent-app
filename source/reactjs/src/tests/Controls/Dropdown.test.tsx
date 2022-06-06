import * as React from 'react';
import { render, fireEvent, screen } from '@testing-library/react';
import 'jest-styled-components';
import DropdownButton from '../../Controls/DropdownButton';

describe('DropdownButton', () => {
    it('DropdownButton renders correctly', () => {
        const wrapper = render(<DropdownButton />);
        expect(wrapper).toMatchSnapshot();
    });
    it('DropdownButton renders correctly', () => {
        const menuitem = ['Op1', 'Op2'];
        const wrapper = render(
            <DropdownButton
                className=''
                menuItems={menuitem}
                text={'Test'}
                title={'Test Drop down Button'}
                primary={true}
                disabled={true}
            />
        );
        expect(wrapper.getByRole('button')).toBeTruthy;
    });
});
