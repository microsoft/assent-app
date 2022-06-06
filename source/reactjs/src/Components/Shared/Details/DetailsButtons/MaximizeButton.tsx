import * as React from 'react';
import { Reducer } from 'redux';
import { IconButton } from '@fluentui/react';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { withContext, Context } from '@micro-frontend-react/employee-experience/lib/Context';
import * as Styled from './DetailsButtonsStyled';
import { toggleDetailsScreen } from '../../SharedComponents.actions';
import {
    sharedComponentsReducerName,
    sharedComponentsReducer,
    sharedComponentsInitialState
} from '../../SharedComponents.reducer';
import { sharedComponentsSagas } from '../../SharedComponents.sagas';
import { TooltipHost } from '@fluentui/react/lib/Tooltip';
import { IComponentsAppState } from '../../SharedComponents.types';

function MaximizeButton(props: { callbackOnMaximizeToggle?(): void }): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const sharedState = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );

    const iconName = sharedState.toggleDetailsScreen ? 'ChromeRestore' : 'FullScreen';
    const title = sharedState.toggleDetailsScreen ? 'Restore' : 'Maximize';

    return (
        <TooltipHost
            content={`${title}`}
            calloutProps={Styled.tooltipCalloutProps}
            styles={Styled.tooltipHostContainer}
        >
            <IconButton
                iconProps={{ iconName }}
                ariaLabel={`Click here to ${title} the details screen`}
                style={Styled.DetailActionIcon}
                onClick={(): void => {
                    dispatch(toggleDetailsScreen());
                    if (props.callbackOnMaximizeToggle) {
                        props.callbackOnMaximizeToggle();
                    }
                }}
            />
        </TooltipHost>
    );
}

const connected = withContext(MaximizeButton);
export { connected as MaximizeButton };
