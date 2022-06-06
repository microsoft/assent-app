/* eslint-disable prettier/prettier */
import * as React from 'react';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import {
    sharedComponentsReducerName,
    sharedComponentsReducer,
    sharedComponentsInitialState
} from '../Shared/SharedComponents.reducer';
import { IComponentsAppState } from '../Shared/SharedComponents.types';
import { sharedComponentsSagas } from '../Shared/SharedComponents.sagas';
import { Reducer } from 'redux';
import { requestMyProfile, requestMyOutOfSyncSummary, updateSelectedSummarytoOutOfSync, updateFilterValue } from '../Shared/SharedComponents.actions';
import { usePageTracking } from '@micro-frontend-react/employee-experience/lib/usePageTracking';
import { usePageTitle } from '@micro-frontend-react/employee-experience/lib/usePageTitle';
import { Stack } from '@fluentui/react/lib/Stack';
import { Summary } from '../Summary/Summary';
import { PrimaryHeader } from '../Shared/Components/PrimaryHeader/PrimaryHeader';
import DetailsFooter from '../Shared/Details/DetailsFooter';
import { getFeature, getPageLoadFeature } from '@micro-frontend-react/employee-experience/lib/UsageTelemetryHelper';
interface IOutOfSyncProps {
    match: {
        params: {
            documentNumber: number;
        }
    }
}
function OutOfSyncPage(props: IOutOfSyncProps): React.ReactElement {
    usePageTitle(`Out of Sync Approvals - ${__APP_NAME__}`);
    const feature = getFeature('MSApprovalsWeb', 'OutofSyncApprovals')
    usePageTracking(getPageLoadFeature(feature));
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);

    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const { isCardViewSelected, isBulkSelected, selectedApprovalRecords } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );

    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth
    });


    React.useEffect((): any => {
        // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
        function handleResize() {
            setDimensions({
                height: window.innerHeight,
                width: window.innerWidth
            });
        }
        window.addEventListener('resize', handleResize);

        // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
        return (_: any) => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    React.useEffect(() => {
        dispatch(requestMyProfile());
        dispatch(requestMyOutOfSyncSummary());
        dispatch(updateSelectedSummarytoOutOfSync());
        dispatch(updateFilterValue('All'));
    }, [dispatch]);

    return (
        <div>
            {isCardViewSelected ? (<Stack>
                <Stack.Item>
                    <Summary windowHeight={dimensions.height}
                        windowWidth={dimensions.width} queryDocNumber={props.match.params.documentNumber.toString()} />

                </Stack.Item>
            </Stack>) : <h1>TABLE VIEW</h1>}
            {isBulkSelected && selectedApprovalRecords.length > 0 && <DetailsFooter />}
        </div>
    );
}

const connected = withContext(OutOfSyncPage);
export { connected as OutOfSyncPage };
