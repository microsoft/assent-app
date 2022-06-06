import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Context, withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import {
    Dropdown,
    Icon,
    IDropdownOption,
    IStackTokens,
    Pivot,
    PivotItem,
    Stack,
    Toggle,
    TooltipHost,
} from '@fluentui/react';
import * as React from 'react';
import { Reducer } from 'redux';
import { sharedComponentsReducerName, sharedComponentsReducer } from '../SharedComponents.reducer';
import { sharedComponentsSagas } from '../SharedComponents.sagas';
import { ClearUserPreferencesAPIMessages, SaveUserPreferencesRequest } from '../SharedComponents.actions';
import { GroupingBy } from './GroupingBy';

import * as Styled from '../SharedLayout';

import * as ButtonStyled from '../Details/DetailsButtons/DetailsButtonsStyled';
import {
    DETAILS_DEFAULT_VIEW,
    DOCKED_VIEW,
    FLYOUT_VIEW,
    GROUP_BY_FILTER,
    HISTORY_DEFAULT_VIEW,
    DEFAULT_VIEW_TYPE,
    DEFAULT_TENANT,
    TABLE_VIEW,
    CARD_VIEW,
} from '../SharedConstants';
import { useRef } from 'react';
import {
    getDetailsDefaultView,
    getHistoryDefaultView,
    getSelectedPage,
    getUserPreferences,
    getUserPreferencesFailureMessage,
    getUserPreferencesSuccessMessage,
    getTenantInfo,
    getSummary,
    getDefaultTenant,
    getAllBulkTenantOptions,
    getDefaultViewType,
} from '../SharedComponents.selectors';
import { trackFeatureUsageEvent, TrackingEventId } from '../../../Helpers/telemetryHelpers';

function UserSettings(): React.ReactElement {
    const stackTokens: IStackTokens = { childrenGap: 12, padding: '20px 0px' };

    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);

    const { useSelector, dispatch, authClient, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const tenantInfo = useSelector(getTenantInfo);
    const summary = useSelector(getSummary);
    const DefaultTenant = useSelector(getDefaultTenant);
    const selectedPage = useSelector(getSelectedPage);
    const userPreferences = useSelector(getUserPreferences);
    const detailsDefaultView = useSelector(getDetailsDefaultView);
    const historyDefaultView = useSelector(getHistoryDefaultView);
    const defaultViewType = useSelector(getDefaultViewType);
    const userPreferencesSuccessMessage = useSelector(getUserPreferencesSuccessMessage);
    const userPreferencesFailureMessage = useSelector(getUserPreferencesFailureMessage);

    const [localDetailsDefaultView, setLocalDetailsDefaultView] = React.useState(false);
    const [localHistoryDefaultView, setLocalHistoryDefaultView] = React.useState(false);
    const [localDefaultViewType, setLocalDefaultViewType] = React.useState(false);
    const [localDefaultTenantView, setLocalDefaultTenantView] = React.useState(null);
    const [dropdownOptions, setDropdownOptions] = React.useState<IDropdownOption[]>([]);
    const [groupBy, setGroupBy] = React.useState(null);

    React.useEffect(() => {
        if (userPreferences && userPreferences.some((a: any) => a.UserPreferenceText === GROUP_BY_FILTER)) {
            const groupByStatus = userPreferences.find(
                (a: any) => a.UserPreferenceText === GROUP_BY_FILTER
            ).UserPreferenceStatus;
            setGroupBy(groupByStatus);
        }
    }, []);

    React.useEffect(() => {
        setLocalDetailsDefaultView(detailsDefaultView == DOCKED_VIEW);
    }, [detailsDefaultView]);

    React.useEffect(() => {
        setLocalHistoryDefaultView(historyDefaultView == DOCKED_VIEW);
    }, [historyDefaultView]);

    React.useEffect(() => {
        setLocalDefaultViewType(defaultViewType == TABLE_VIEW);
    }, [defaultViewType]);

    React.useEffect(() => {
        setLocalDefaultTenantView(DefaultTenant);
    }, [DefaultTenant]);

    React.useEffect(() => {
        populateDropDownOptions();
    }, [summary, selectedPage]);

    const populateDropDownOptions = (): void => {
        getFilteredMenuItems(GroupingBy.Tenant);
    };

    const getFilteredMenuItems = (filterKey: any): void => {
        const filterMenuProps = getAllBulkTenantOptions(tenantInfo);
        if (filterMenuProps) {
            setDropdownOptions(filterMenuProps);
        }
    };

    const menuProps: IDropdownOption[] = [
        {
            key: GroupingBy.Tenant,
            text: 'Application',
        },
        {
            key: GroupingBy.Submitter,
            text: 'Submitter',
        },
        {
            key: GroupingBy.Date,
            text: 'Date',
        },
    ];

    const saveUserPreferences = () => {
        const existingFeaturePreferences = userPreferences || [];

        if (existingFeaturePreferences) {
            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === DETAILS_DEFAULT_VIEW)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === DETAILS_DEFAULT_VIEW
                ).UserPreferenceStatus = localDetailsDefaultView ? DOCKED_VIEW : FLYOUT_VIEW;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: DETAILS_DEFAULT_VIEW,
                    UserPreferenceStatus: localDetailsDefaultView ? DOCKED_VIEW : FLYOUT_VIEW,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === HISTORY_DEFAULT_VIEW)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === HISTORY_DEFAULT_VIEW
                ).UserPreferenceStatus = localHistoryDefaultView ? DOCKED_VIEW : FLYOUT_VIEW;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: HISTORY_DEFAULT_VIEW,
                    UserPreferenceStatus: localHistoryDefaultView ? DOCKED_VIEW : FLYOUT_VIEW,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === DEFAULT_VIEW_TYPE)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === DEFAULT_VIEW_TYPE
                ).UserPreferenceStatus = localDefaultViewType ? TABLE_VIEW : CARD_VIEW;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: DEFAULT_VIEW_TYPE,
                    UserPreferenceStatus: localDefaultViewType ? TABLE_VIEW : CARD_VIEW,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === GROUP_BY_FILTER)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === GROUP_BY_FILTER
                ).UserPreferenceStatus = groupBy;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: GROUP_BY_FILTER,
                    UserPreferenceStatus: groupBy,
                });
            }

            if (existingFeaturePreferences.some((a: any) => a.UserPreferenceText === DEFAULT_TENANT)) {
                existingFeaturePreferences.find(
                    (a: any) => a.UserPreferenceText === DEFAULT_TENANT
                ).UserPreferenceStatus = localDefaultTenantView;
            } else {
                existingFeaturePreferences.push({
                    UserPreferenceText: DEFAULT_TENANT,
                    UserPreferenceStatus: localDefaultTenantView,
                });
            }
        }

        const request: any = {
            FeaturePreferenceJson: JSON.stringify(existingFeaturePreferences),
        };

        dispatch(SaveUserPreferencesRequest(request));
    };

    const isMounted = useRef(false);

    React.useEffect(() => {
        dispatch(ClearUserPreferencesAPIMessages());
    }, []);

    React.useEffect(() => {
        if (isMounted.current) {
            saveUserPreferences();
        }
    }, [localDetailsDefaultView, localHistoryDefaultView, localDefaultViewType, groupBy, localDefaultTenantView]);

    const detailsDefaultViewChange = (ev: React.MouseEvent<HTMLElement>, checked: boolean) => {
        isMounted.current = true;
        setLocalDetailsDefaultView(checked);
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'DetailsDefaultViewToggle',
            checked ? 'MSApprovals.DetailsDefaultView.Docked' : 'MSApprovals.DetailsDefaultView.Flyout',
            checked ? TrackingEventId.DockedView : TrackingEventId.FlyOutView,
            null
        );
    };

    const historyDefaultViewChange = (ev: React.MouseEvent<HTMLElement>, checked: boolean) => {
        isMounted.current = true;
        setLocalHistoryDefaultView(checked);
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'HistoryDefaultViewToggle',
            checked ? 'MSApprovals.HistoryDefaultView.Docked' : 'MSApprovals.HistoryDefaultView.Flyout',
            checked ? TrackingEventId.DockedView : TrackingEventId.FlyOutView,
            null
        );
    };

    const defaultViewTypeChange = (ev: React.MouseEvent<HTMLElement>, checked: boolean) => {
        isMounted.current = true;
        setLocalDefaultViewType(checked);
        trackFeatureUsageEvent(
            authClient,
            telemetryClient,
            'DefaultViewTypeToggle',
            checked ? 'MSApprovals.DefaultViewType.Table' : 'MSApprovals.DefaultViewType.Card',
            checked ? TrackingEventId.TableView : TrackingEventId.CardView,
            null
        );
    };

    const defaultTenantChange = (ev: React.FormEvent<HTMLElement>, value: IDropdownOption) => {
        isMounted.current = true;
        setLocalDefaultTenantView(value.key);
    };

    const groupByChange = (ev: React.FormEvent<HTMLElement>, value: IDropdownOption) => {
        isMounted.current = true;
        setGroupBy(value.key);
    };

    return (
        <Pivot aria-label="Settings">
            <PivotItem headerText="General">
                <Stack tokens={stackTokens}>
                    <Stack.Item>
                        {userPreferencesFailureMessage && (
                            <Styled.ErrorMessage>{userPreferencesFailureMessage}</Styled.ErrorMessage>
                        )}
                        {userPreferencesSuccessMessage && (
                            <Styled.SuccessMessage>{userPreferencesSuccessMessage}</Styled.SuccessMessage>
                        )}
                    </Stack.Item>
                    <Stack.Item>
                        <label>
                            Choose default view
                            <TooltipHost
                                content="Select between viewing the details page as a flyout or docked."
                                calloutProps={ButtonStyled.tooltipCalloutProps}
                                styles={ButtonStyled.tooltipHostContainer}
                            >
                                <Icon
                                    ariaLabel="Select between viewing the details page as a flyout or docked."
                                    iconName="info"
                                    tabIndex={0}
                                />
                            </TooltipHost>
                        </label>
                    </Stack.Item>
                    <Stack.Item>
                        <Toggle
                            inlineLabel
                            label="Details view"
                            onText="Docked"
                            offText="Flyout"
                            onChange={detailsDefaultViewChange}
                            checked={localDetailsDefaultView}
                            styles={{ label: { width: '80px' } }}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <Toggle
                            inlineLabel
                            label="History view"
                            onText="Docked"
                            offText="Flyout"
                            onChange={historyDefaultViewChange}
                            checked={localHistoryDefaultView}
                            styles={{ label: { width: '80px' } }}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <Toggle
                            inlineLabel
                            label="View Type"
                            onText="Table"
                            offText="Card"
                            onChange={defaultViewTypeChange}
                            checked={localDefaultViewType}
                            styles={{ label: { width: '80px' } }}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <Dropdown
                            placeholder="Select an option"
                            label="Default grouping for pending approvals"
                            options={menuProps}
                            onChange={groupByChange}
                            selectedKey={groupBy}
                        />
                    </Stack.Item>
                    <Stack.Item>
                        <Dropdown
                            placeholder="Select an option"
                            label="Default Application for Bulk Approval"
                            options={dropdownOptions}
                            onChange={defaultTenantChange}
                            selectedKey={localDefaultTenantView}
                            styles={Styled.dropdownStyles}
                        />
                    </Stack.Item>
                </Stack>
            </PivotItem>
            {/* <PivotItem headerText="Language">TBD</PivotItem> */}
        </Pivot>
    );
}

const connected = withContext(UserSettings);
export { connected as UserSettings };
