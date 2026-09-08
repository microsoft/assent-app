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
import { NavigationUtils } from '../../Utils/NavigationUtils';

interface ICloseButtonProps {
    action?(): void;
}

function CloseButton(props: ICloseButtonProps): React.ReactElement {
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
                                // Use NavigationUtils to preserve dashboard route and other parameters
                                const currentParams = NavigationUtils.getCurrentUrlParams(history);
                                const basePath = NavigationUtils.getBasePath(history);
                                const alias = currentParams.get('alias');
                                const filterParam = currentParams.get('filter');
                                
                                const queryParams = new URLSearchParams();
                                
                                if (alias) {
                                    queryParams.set('alias', alias);
                                }
                                
                                if (filterParam) {
                                    queryParams.set('filter', filterParam);
                                }
                                
                                const queryString = queryParams.toString();
                                const queryPath = queryString ? `?${queryString}` : '';
                                
                                history.push(`${basePath}${queryPath}`);
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

const connected = withContext(CloseButton as any);
export { connected as CloseButton };