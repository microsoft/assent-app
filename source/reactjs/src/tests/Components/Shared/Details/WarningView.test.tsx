import * as React from 'react';
import { render} from '@testing-library/react';
import WarningView from '../../../../Components/Shared/Details/DetailsMessageBars/WarningView'

describe('WarningView', () => {
    it('should render Warning view correctly with warning title', () => {
        const wrapper = render(
            <WarningView
            warningTitle={''}
            warningMessages= {null}
            onDismiss={null}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });

    it('should render Warning view correctly with undefined props', () => {
        const wrapper = render(
            <WarningView
            warningTitle={undefined}
            warningMessages= {undefined}
            onDismiss={undefined}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });

    it('should render Warning view correctly when onDismiss and warningMessages is undefined', () => {
        const wrapper = render(
            <WarningView
            warningTitle={"warningTitle"}
            warningMessages= {undefined}
            onDismiss={undefined}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });

    it('should render Warning view correctly when warningMessage is defined' , () => {
        const wrapper = render(
            <WarningView
            warningTitle={"warningTitle"}
            warningMessages= {['A','B']}
            onDismiss={undefined}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });
});