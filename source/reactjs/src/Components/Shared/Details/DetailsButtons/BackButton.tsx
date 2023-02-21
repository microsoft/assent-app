import * as React from 'react';
import * as Styled from './DetailsButtonsStyled';
import { Reducer } from 'redux';
import { IconButton } from '@fluentui/react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { sharedComponentsReducerName, sharedComponentsReducer } from '../../SharedComponents.reducer';
import { sharedComponentsSagas } from '../../SharedComponents.sagas';
import { clearDocumentPreview, closeDocumentPreview, closeFileUpload, closeMicrofrontend } from '../Details.actions';
import { TooltipHost } from '@fluentui/react/lib/Tooltip';

function BackButton(props: { callbackOnBackButton?(): void }): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    const { dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    return (
        <TooltipHost content="Back" calloutProps={Styled.tooltipCalloutProps} styles={Styled.tooltipHostContainer}>
            <IconButton
                iconProps={{ iconName: 'Back' }}
                title="Go Back"
                ariaLabel="Go Back"
                onClick={(): void => {
                    dispatch(clearDocumentPreview());
                    dispatch(closeDocumentPreview());
                    dispatch(closeFileUpload());
                    dispatch(closeMicrofrontend());
                    if (props.callbackOnBackButton) {
                        props.callbackOnBackButton();
                    }
                }}
            />
        </TooltipHost>
    );
}

const connected = withContext(BackButton);
export { connected as BackButton };