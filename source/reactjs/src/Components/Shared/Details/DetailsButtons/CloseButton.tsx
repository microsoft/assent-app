import * as React from 'react';
import { Reducer } from 'redux';
import { IconButton } from '@fluentui/react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import * as Styled from './DetailsButtonsStyled';
import { updatePanelState } from '../../SharedComponents.actions';
import {
    sharedComponentsReducerName,
    sharedComponentsInitialState,
    sharedComponentsReducer
} from '../../SharedComponents.reducer';
import { sharedComponentsSagas } from '../../SharedComponents.sagas';
import { IComponentsAppState } from '../../SharedComponents.types';
import { detailsInitialState, detailsReducerName } from '../../Details/Details.reducer';
import { IDetailsAppState } from '../../Details/Details.types';
import { TooltipHost } from '@fluentui/react/lib/Tooltip';
import { useHistory, useLocation } from 'react-router-dom';

function CloseButton(props: { action?(): void }): React.ReactElement {
    const history = useHistory();
    const location = useLocation();
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { selectedSummaryTileRef } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );

    const { displayDocumentNumber } = useSelector(
        (state: IDetailsAppState) => state.dynamic?.[detailsReducerName] || detailsInitialState
    );

    let closeButtonRef: any = null;

    React.useEffect(() => {
        if (closeButtonRef && displayDocumentNumber) {
            closeButtonRef.focus();
            document.body.classList.add('ms-Fabric--isFocusVisible');
        }
    }, [closeButtonRef, displayDocumentNumber]);

    return (
        <TooltipHost content="Close" calloutProps={Styled.tooltipCalloutProps} styles={Styled.tooltipHostContainer}>
            <IconButton
                id="detail-close-btn"
                className="close-button"
                componentRef={input => (closeButtonRef = input)}
                iconProps={{ iconName: 'Cancel' }}
                title="Close"
                ariaLabel="Click here to close the details card"
                style={Styled.DetailActionIcon}
                onClick={() => {
                    if (props.action) {
                        props.action();
                    } else {
                        if (history && location) {
                            if (
                                location.pathname.length > 1 &&
                                !location.pathname?.toLowerCase()?.includes('history')
                            ) {
                                history.push('/');
                            }
                        }
                        dispatch(updatePanelState(false));
                        document.getElementById(selectedSummaryTileRef)?.focus();
                    }
                }}
            />
        </TooltipHost>
    );
}

const connected = withContext(CloseButton);
export { connected as CloseButton };