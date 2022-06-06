import * as React from 'react';
import { render, fireEvent, screen } from '@testing-library/react';
import BasicButton from '../../Controls/BasicButton';

describe('BasicButton', () => {
    it('should BasicButton renders correctly', () => {
        const wrapper = render(<BasicButton />);
        const button = wrapper.container.querySelector('button');
        expect(wrapper).toMatchSnapshot();
        expect(button).toBeTruthy;
    });

    it('should primary Button renders correctly', () => {
        const onClick = () => {};
        const wrapper = render(
            <BasicButton
                text={'Test'}
                title={'Test button'}
                primary={true}
                onClick={onClick}
                componentRef={null}
                disabled={false}
            />
        );
        expect(wrapper.getByTitle('Test button')).toBeTruthy;
    });

    it('Verify BasicButton click event', () => {
        const text = 'test';
        const { getByText } = render(<BasicButton text={text} title={'Testbtn'} primary={true} />);
        const button = getByText(text).closest('button');
        fireEvent.click(button);
    });

    it('should BasicButton click once', () => {
        const handleClick = jest.fn();
        const button = render(<BasicButton title={'Testbtn'} onClick={handleClick} primary={true} />);
        fireEvent.click(button.getByTitle('Testbtn'));
        expect(handleClick).toHaveBeenCalledTimes(1);
    });
});
