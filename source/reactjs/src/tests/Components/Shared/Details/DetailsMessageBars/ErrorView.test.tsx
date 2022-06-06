import * as React from 'react';
import { render } from '@testing-library/react';
import ErrorView from '../../../../../Components/Shared/Details/DetailsMessageBars/ErrorView'

describe('ErrorView', () => {
    it('error view should render properly when all properties are given', () => {
        const wrapper = render(
            <ErrorView
            errorMessage={''}
            failureType={'Execution'}
            linkHref={null}
            linkText = {null}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });

    it('error view should render without linkText and linkHref ', () => {
        const wrapper = render(
            <ErrorView
            errorMessage={''}
            failureType={'Execution'}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });


    it('error view should render properly when  linkHref is null', () => {
        const wrapper = render(
            <ErrorView
            errorMessage={undefined}
            failureType={undefined}
            linkHref={null}
            linkText = {'string'}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });

    it('error view should render properly when linkHref and linkText present', () => {
        const wrapper = render(
            <ErrorView
            errorMessage={undefined}
            failureType={undefined}
            linkHref={'string'}
            linkText = {'string'}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });
 });
