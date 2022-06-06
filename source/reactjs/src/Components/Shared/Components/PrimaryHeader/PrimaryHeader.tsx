/* eslint-disable prettier/prettier */
import * as React from 'react';
import * as HeaderStyled from './PrimaryHeaderStyling';
import '../../../../App.css';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import {
    sharedComponentsReducerName,
    sharedComponentsReducer
} from '../../SharedComponents.reducer';
import { IComponentsAppState } from '../../SharedComponents.types';
import { sharedComponentsSagas } from '../../SharedComponents.sagas';
import { Reducer } from 'redux';
import {
    updateFilterValue,
    updateBulkUploadConcurrentValue,
    updateBulkSelected,
    updateApprovalRecords,
    updateGroupedSummary,
    updateBulkFailedStatus,
    requestPullTenantSummary,
    requestExternalTenantInto,
    requestMyDelegations,
    updateCardViewType
} from '../../SharedComponents.actions';
import {
    getSummaryGroupedBy,
    getIsLoadingSummary,
    getTenantInfo,
    getSummary,
    getSelectedApprovalRecords,
    getIsBulkSelected,
    getFilterValue,
    getSelectedPage,
    getHistoryGroupedBy,
    getDefaultTenant,
    getBulkFilteredDropDownMenuItems,
    getCardViewSelected,
    getIsPanelOpen,
    getProfile,
    getBulkTenantsFromSummary,
    getPullTenantSummaryCount,
    getFilteredTenantInfo,
    getPullTenantSummaryData,
    getToggleDetailsScreen
} from '../../SharedComponents.selectors';
import { setBulkMessagebarHeight } from '../../Details/Details.actions';
import { Stack } from '@fluentui/react/lib/Stack';
import { Checkbox } from '@fluentui/react/lib/Checkbox';
import { Dropdown, IDropdownOption } from '@fluentui/react';
import { sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer, SharedComponentsPersistentInitialState } from '../../SharedComponents.persistent-reducer';
import { usePersistentReducer } from '../PersistentReducer';
import { GroupingBy } from '../GroupingBy';
import { MessageBar } from '@fluentui/react';
import { isMobile } from 'react-device-detect';
import { trackFeatureUsageEvent, TrackingEventId } from '../../../../Helpers/telemetryHelpers';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

function PrimaryHeader(props: any): React.ReactElement {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    usePersistentReducer(sharedComponentsPersistentReducerName, sharedComponentsPersistentReducer);

    const { useSelector, dispatch,  authClient, telemetryClient } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const summaryGroupedBy = useSelector(getSummaryGroupedBy);
    const isLoadingSummary = useSelector(getIsLoadingSummary);
    const tenantInfo = useSelector(getTenantInfo);
    const summary = useSelector(getSummary);
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
    const isBulkSelected = useSelector(getIsBulkSelected);
    const filterValue = useSelector(getFilterValue);
    const selectedPage = useSelector(getSelectedPage);
    const historyGroupedBy = useSelector(getHistoryGroupedBy);
    const DefaultTenant = useSelector(getDefaultTenant);
    const isCardViewSelected = useSelector(getCardViewSelected);
    const isPanelOpen = useSelector(getIsPanelOpen);
    const profile = useSelector(getProfile);
    const pullTenantSummaryCount = useSelector(getPullTenantSummaryCount);
    const filteredTenantInfo = useSelector(getFilteredTenantInfo);
    const pullTenantSummaryData = useSelector(getPullTenantSummaryData);
    const toggleDetailsScreen = useSelector(getToggleDetailsScreen);

    const { userAlias } = useSelector(
        (state: IComponentsAppState) => state.SharedComponentsPersistentReducer || SharedComponentsPersistentInitialState
    );

    const [allHistory] = React.useState<any>(null);
    const [dropdownOptions, setDropdownOptions] = React.useState<IDropdownOption[]>([]);
    const [selectedDataType, setSelectedKey] = React.useState<string>('All');
    const [availableBulkRecords, setAvailableBulkRecords] = React.useState<boolean>(true);
    const [largestOption, setLargestOption] = React.useState<string>('');
    const [preservedGrouping, setPreservedGrouping] = React.useState(summaryGroupedBy);
    const [largestOptionWidth, setLargestOptionWidth] = React.useState<string>('0px');

    const controlsAndComplianceRequired = filteredTenantInfo?.isControlsAndComplianceRequired ?? true;

    const bulkCheckboxLabel = 'Select multiple'

    const isMaximized = isPanelOpen && toggleDetailsScreen;

    const getFilteredDropDownMenuItems = (): IDropdownOption[] => {

        const filterMenuProps = getBulkFilteredDropDownMenuItems(summary, tenantInfo);
        const bulkRecordsFlag = filterMenuProps.length > 0
        setAvailableBulkRecords(bulkRecordsFlag);
        return filterMenuProps;
    }

    React.useEffect(() => {
        if (filterValue == "All") {
            setSelectedKey('');
        }
    }, [allHistory, filterValue, historyGroupedBy]);

    React.useEffect(() => {
        if (!isBulkSelected) {
            setSelectedKey('');
        }
    }, [isBulkSelected]);

    React.useEffect(() => {
        if (isBulkSelected) {
            let dropdownOptions = getFilteredDropDownMenuItems();
            if (dropdownOptions.length > 0) {
                const defaultDropDownItem = dropdownOptions.find(x => x.key === DefaultTenant) || dropdownOptions[0];
                displaySelectedTenantRecords(defaultDropDownItem);
            }
        }
    }, [DefaultTenant]);

    React.useEffect(() => {
        if (tenantInfo && summary) {
            setDropdownOptions(getFilteredDropDownMenuItems());
        }
    }, [summary, tenantInfo]);


    React.useEffect(() => {
        setDropdownOptions(getFilteredDropDownMenuItems());
    }, [summaryGroupedBy, selectedPage]);

    React.useEffect(() => {
        let optionLength = 0;
        let largeOption;
        dropdownOptions?.map(function (item: any) {
            if (item.text.length > optionLength) {
                optionLength = item.text.length;
                largeOption = item.text;
            }
        }
        );
        if (largestOption != largeOption) {
            setLargestOption(largeOption);
            setLargestOptionWidth(getTextWidth(largeOption));
        }
    }, [dropdownOptions]);

    

    const clearBulkRecords = (): void => {
        for (let i = 0; i < selectedApprovalRecords.length; i++) {
            const selRecord = selectedApprovalRecords[i];
            updateCardStyle(false, selRecord.DocumentNumber);
        }
        dispatch(updateApprovalRecords([]));
        dispatch(setBulkMessagebarHeight(0));
        dispatch(updateBulkFailedStatus(false));
    }

    const getSelectedRecords = (event: React.FormEvent<HTMLDivElement>, item: IDropdownOption): void => {
        displaySelectedTenantRecords(item);
        clearBulkRecords();
    };

    const updateCardStyle = (selected: boolean, elementID: any): void => {
        const element = document.getElementById(elementID);
        if (element) {
            const removeClass = selected ? 'defaultCardStyle' : 'bulkSelectedCardStyle';
            const addClass = selected ? 'bulkSelectedCardStyle' : 'defaultCardStyle';
            element.classList.remove(removeClass);
            element.classList.add(addClass);
        }
    }

    const getTextWidth = (inputText: string): string => {

        const font = "14px Segoe UI";
        const offSetWidth = 40;
        const canvas = document.createElement("canvas");
        const context = canvas.getContext("2d");
        context.font = font;
        const width = context.measureText(inputText).width;
        canvas.parentNode?.removeChild(canvas);
        return Math.ceil(width + offSetWidth) + "px";
    }

    const onCheckboxChange = (ev: React.FormEvent<HTMLElement>, checked: boolean): void => {
        dispatch(updateBulkSelected(checked));
        if (!checked) {
            clearBulkRecords();
            dispatch(updateFilterValue('All'));
            dispatch(updateGroupedSummary(preservedGrouping));
            setSelectedKey('');
        } else if (checked) {           
            setPreservedGrouping(summaryGroupedBy);
            dispatch(updateGroupedSummary(GroupingBy.Tenant));
            const options = getFilteredDropDownMenuItems();
            setDropdownOptions(options);
            if (options.length > 0) {
                const cardViewOptions = getBulkTenantsFromSummary(summary, tenantInfo, true);
                const pullCountForFirst = pullTenantSummaryCount.find(x => x.AppName === options[0].key)?.Count;
                const isFirstValidForPull = typeof pullCountForFirst === 'number' && pullCountForFirst > 0;
                const isFirstValid = cardViewOptions?.[0]?.key === options[0].key || isFirstValidForPull;
                const firstOption = isFirstValid ? options[0] : cardViewOptions?.[0] ?? options[0];
                const defaultDropDownItem =
                    !userAlias && DefaultTenant
                        ? options.find(x => x.key === DefaultTenant) || firstOption
                        : firstOption;
                displaySelectedTenantRecords(defaultDropDownItem);
            } else {
                setAvailableBulkRecords(false);
            }
        }

        trackFeatureUsageEvent(authClient, telemetryClient, `Bulk Approval - ${checked}`, "MSApprovals.BulkApproval", TrackingEventId.BulkApproval, null)
    }

    const displaySelectedTenantRecords = (tenantItem: any): void => {
        if (summary && tenantInfo) {
            const selectedTenant = tenantInfo.find((tenant: any) => tenant.appName === tenantItem.key);
            const isExternalTenantActionDetails = selectedTenant.isExternalTenantActionDetails;
            const tenantId = selectedTenant.tenantId;
            if (selectedTenant.isPullModelEnabled) {
                setSelectedKey(tenantItem.key.toString());
                if (isCardViewSelected) {
                    dispatch(updateCardViewType(false));
                }
                dispatch(updateFilterValue(tenantItem.key.toString()));
                dispatch(updateBulkSelected(true));
                dispatch(requestPullTenantSummary(tenantId, userAlias, null, isExternalTenantActionDetails));
                dispatch(requestMyDelegations(profile?.userPrincipalName, tenantId, tenantItem.key.toString()));
                dispatch(updateBulkUploadConcurrentValue(selectedTenant ? selectedTenant.bulkActionConcurrentCall : 0));
            } else {
                setSelectedKey(tenantItem.key.toString());
                dispatch(updateFilterValue(tenantItem.key.toString()));
                if (isExternalTenantActionDetails) {
                    dispatch(requestExternalTenantInto(tenantId, userAlias));
                }
                dispatch(updateBulkUploadConcurrentValue(selectedTenant ? selectedTenant.bulkActionConcurrentCall : 0));
                dispatch(updateBulkSelected(true));
            }
        }
    }

    const setBulkMessageBarRef = (element: any): void => {
        const bulkMessageElement = element;
        if (bulkMessageElement && bulkMessageElement.clientHeight) {
            if (bulkMessageHeight != bulkMessageElement.clientHeight) {
                bulkMessageHeight = bulkMessageElement.clientHeight;
                dispatch(setBulkMessagebarHeight(bulkMessageElement.clientHeight));
            }
        }

    }

    const renderBulkCheckBox = () => {
        return (<Checkbox
            label={bulkCheckboxLabel}
            checked={isBulkSelected}
            disabled={isLoadingSummary && (!(tenantInfo?.length > 0) || (!availableBulkRecords))}
            onChange={onCheckboxChange}
        />);
    }

    const renderBulkDropDown = () => {
        return (<Dropdown
            styles={HeaderStyled.DropDownStyle(largestOptionWidth, isMaximized, isPanelOpen)}
            placeholder="Select an application"
            title="Bulk Filter"
            label="for Application"
            aria-label="for Application"
            options={dropdownOptions}
            onChange={getSelectedRecords}
            selectedKey={(selectedDataType === '' && filterValue !== 'All') ? filterValue : selectedDataType}
            disabled={!isBulkSelected || (!availableBulkRecords)}
        />
        );
    };

    const renderComponents = () => {
        return (<HeaderStyled.SecondaryHeaderContainer>
            <Stack horizontal styles={HeaderStyled.SecondaryHeaderStackStyles}>

                <Stack.Item
                    grow={0.05}
                    align="start"
                    styles={HeaderStyled.GroupAndFilterIconStackItemStyles}
                >
                    <Stack horizontalAlign="start" verticalAlign="center" horizontal wrap tokens={{childrenGap: 5}}>
                        {availableBulkRecords && renderBulkCheckBox()}
                        {availableBulkRecords && isBulkSelected && renderBulkDropDown()}
                        {tenantInfo?.length > 0 && !availableBulkRecords && (<p>None of your pending requests are eligible for bulk approvals</p>)}
                    </Stack>

                </Stack.Item>
            </Stack>
        </HeaderStyled.SecondaryHeaderContainer>)
    }

    const renderMobileViewComponents = () => {
        return (<HeaderStyled.SecondaryHeaderContainer>
            <Stack horizontal styles={HeaderStyled.SecondaryHeaderMobileStackStyles}>
                <Stack.Item
                    grow={0.05}
                    align="center"
                    styles={HeaderStyled.GroupAndFilterIconStackItemStyles}
                >
                    <Stack verticalAlign="center" horizontal>
                        {availableBulkRecords && renderBulkCheckBox()}
                        {tenantInfo?.length > 0 && !availableBulkRecords && (<p>None of your pending requests are eligible for bulk approvals</p>)}
                    </Stack>
                    <Stack verticalAlign="center" horizontal>
                        {availableBulkRecords && isBulkSelected && renderBulkDropDown()}
                    </Stack>
                </Stack.Item>
            </Stack>
        </HeaderStyled.SecondaryHeaderContainer>)
    }

    let bulkMessageHeight = 0;

    return (
        <div style={(isBulkSelected && isPanelOpen) ? { marginBottom: '0.75%' } : isBulkSelected ? { marginBottom: '0.5%' } : null}>
            {isBulkSelected && tenantInfo?.length > 0 && availableBulkRecords
                && controlsAndComplianceRequired && (
                    <HeaderStyled.BulkMessageHeight
                        ref={element => setBulkMessageBarRef(element)}
                    >
                        <MessageBar
                            styles={HeaderStyled.DelegationBarStyles}
                            isMultiline={false}
                            aria-label={'Informational message'}
                        >
                            <Stack.Item>
                                <HeaderStyled.MessageBarText>
                                    To comply with MS policy requirements, some applications may require you to click the request and view the full details before a request is enabled is for bulk action
                                </HeaderStyled.MessageBarText>
                            </Stack.Item>
                        </MessageBar>
                    </HeaderStyled.BulkMessageHeight>)}
            {isMobile && window.innerWidth < 800 ? renderMobileViewComponents() : renderComponents()}

        </div>
    );
}

const connected = withContext(PrimaryHeader);
export { connected as PrimaryHeader };