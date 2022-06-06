import * as React from 'react';
import * as SummaryStyled from '../../Summary/SummaryStyling';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { detailsInitialState, detailsReducerName } from './Details.reducer';
import { IDetailsAppState } from './Details.types';

import { requestFullyScrolled } from './Details.actions';
import { RequestView } from './DetailsAdaptive';
import { getSelectedPage, getToggleDetailsScreen } from '../SharedComponents.selectors';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

interface IDetailsDockedViewProps {
    templateType: string;
    windowHeight: number;
    windowWidth: number;
    historyRef: any;
    locationRef: any;
}

function DetailsDockedViewBase(props: IDetailsDockedViewProps): React.ReactElement {
    const { windowHeight, windowWidth, templateType, historyRef, locationRef } = props;
    const componentContext = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const reduxContext = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const { useSelector, dispatch } = reduxContext;

    const {
        isPreviewOpen,
        isMicrofrontendOpen,
        isRequestFullyScrolled,
        footerHeight,
        bulkMessagebarHeight,
        aliasMessagebarHeight,
        isRequestFullyRendered
    } = useSelector((state: IDetailsAppState) => state.dynamic?.[detailsReducerName] || detailsInitialState);

    const toggleDetailsScreen = useSelector(getToggleDetailsScreen);
    const selectedPage = useSelector(getSelectedPage);

    function handleScroll(e: any): void {
        const threshold = 5;
        const bottom = Math.round(e.target.scrollHeight - e.target.scrollTop) - e.target.clientHeight <= threshold;
        if (!isPreviewOpen && !isMicrofrontendOpen && !isRequestFullyScrolled && isRequestFullyRendered && bottom) {
            dispatch(requestFullyScrolled(true, false)); // Added this code to fix the re-render issue
        }
    }

    return (
        <div
            className={
                ' ms-CustomGrid-col ms-sm12 ms-md12 ms-lg12-docked ms-width50' +
                (toggleDetailsScreen ? ' ms-xl8-docked ms-xxl8-docked ms-xxxl8-docked' : ' ms-xl6-docked')
            }
        >
            <SummaryStyled.DetailCardContainer
                className="custom-details-container"
                windowHeight={windowHeight}
                windowWidth={windowWidth}
                footerHeight={footerHeight}
                onScroll={handleScroll}
                bulkMessagebarHeight={bulkMessagebarHeight}
                aliasMessagebarHeight={aliasMessagebarHeight}
                selectedPage={selectedPage}
            >
                <RequestView
                    componentContext={componentContext}
                    reduxContext={reduxContext}
                    viewType="Docked"
                    templateType={templateType}
                    handleContainerScrolling={handleScroll}
                    windowWidth={windowWidth}
                    windowHeight={windowHeight}
                    historyRef={historyRef}
                    locationRef={locationRef}
                ></RequestView>
            </SummaryStyled.DetailCardContainer>
        </div>
    );
}

const memoizedDetailsDockedView = React.memo(DetailsDockedViewBase);
export { memoizedDetailsDockedView as DetailsDockedView };
