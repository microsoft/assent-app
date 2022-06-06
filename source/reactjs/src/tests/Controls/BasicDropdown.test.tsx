import * as React from 'react';
import BasicDropdown from '../../Controls/BasicDropdown';
import { render, fireEvent, screen, getByLabelText } from '@testing-library/react';

describe('BasicDropdown', () => {
    it('should BasicDropdown renders correctly', () => {
        const wrapper = render(<BasicDropdown />);
        expect(wrapper).toMatchSnapshot();
    });

    it('should BasicDropdown renders correctly with  props', () => {
        const options = [
            { value: 'red', label: 'Red' },
            { value: 'green', label: 'Green' }
        ];
        const wrapper = render(
            <BasicDropdown
                options={options}
                placeholder={''}
                selectedKey={''}
                label={'Test'}
                required={true}
                styles={() => {}}
            />
        );
        expect(wrapper.getByLabelText('Test')).toBeTruthy;
    });

    it('should BasicDropdown onChange Event fire correctly', () => {
        const text = 'test';
        const options = [
            { value: 'red', label: 'Red' },
            { value: 'green', label: 'Green' }
        ];
        const {getByLabelText} = render(
            <BasicDropdown
                options={options}
                placeholder={''}
                selectedKey={'red'}
                label={text}
                required={true}
                styles={() => {}}
            />
        );
        const dropDown = getByLabelText(text) as HTMLInputElement;
        expect(dropDown).toBeTruthy;
        fireEvent.change(dropDown);
    });
});
