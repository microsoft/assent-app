import * as React from 'react';
import { getByLabelText, render, fireEvent } from '@testing-library/react';
import 'jest-styled-components';
import MultilineTextField from '../../Controls/MultilineTextField';

describe('MultilineTextField', () => {
    it('should MultilineTextField renders correctly', () => {
        const wrapper = render(<MultilineTextField />);
        expect(wrapper).toMatchSnapshot();
    });

    it('should MultilineTextField renders correctly with props', () => {
        const wrapper = render(
            <MultilineTextField
                label={'Test'}
                name={'Test'}
                ariaLabel={'Test'}
                rows={1}
                required={true}
                value={''}
                componentRef={''}
                inputErrorMessage={''}
                styles={() => {}}
                ariaLabelledby={''}
            />
        );
        expect(wrapper.getByRole('textbox')).toBeTruthy;
    });

    it('should MultilineTextField onChange event fires', () => {
        const ctrllable = 'test';
        const { getByLabelText } = render(
            <MultilineTextField
                label={ctrllable}
                name={'Test'}
                ariaLabel={'Test'}
                rows={1}
                required={true}
                value={''}
                componentRef={''}
                inputErrorMessage={''}
                styles={() => {}}
                ariaLabelledby={''}
            />
        );
        const textbox = getByLabelText(ctrllable) as HTMLInputElement;
        fireEvent.change(textbox, { target: { value: 'Env' } });
    });
});
