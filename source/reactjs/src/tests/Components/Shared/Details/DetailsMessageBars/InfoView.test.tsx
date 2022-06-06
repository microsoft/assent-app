import * as React from 'react';
import { render } from '@testing-library/react';
import InfoView from '../../../../../Components/Shared/Details/DetailsMessageBars/InfoView'


describe('InfoView', () => {
    it('should render info view when properties are given', () => {
        const wrapper = render(
            <InfoView
            infoTitle= {"string"}
            infoMessage= {"string"}
            linkHref= {"string"}
            linkText= {"string"}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });

    it('error view should render properly when title and message is undefined', () => {
        const wrapper = render(
            <InfoView
            infoTitle={undefined}
            infoMessage= {undefined}
            />
        );
        expect(wrapper).toMatchSnapshot();
    });

});