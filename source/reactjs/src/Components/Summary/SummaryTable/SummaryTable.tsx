/* eslint-disable prefer-const */
import * as React from 'react';
import * as Styled from './SummaryTableStyling';
import { Reducer } from 'redux';
import { useDynamicReducer } from '@micro-frontend-react/employee-experience/lib/useDynamicReducer';
import { Stack } from '@fluentui/react/lib/Stack';
import { sharedComponentsReducerName, sharedComponentsReducer } from '../../Shared/SharedComponents.reducer';
import { sharedComponentsSagas } from '../../Shared/SharedComponents.sagas';
import { detailsReducer, detailsReducerName } from '../../Shared/Details/Details.reducer';
import { detailsSagas } from '../../Shared/Details/Details.sagas';
import SummaryTableColumns from './SummaryTableColumns';
import * as SummaryStyled from '../SummaryStyling';
import {
    Callout,
    Checkbox,
    DefaultButton,
    DirectionalHint,
    Dropdown,
    IconButton,
    IContextualMenuItem,
    IContextualMenuProps,
    IDropdownOption,
    Toggle,
} from '@fluentui/react';
import { pullTenantTableColumns } from './PullTenantColumns';
import { defaultTableColumns, tenantColumns } from './TenantColumns';
import { FilterTags, IFilterTag } from '../Dashboard/FilterTags';
import { forEach } from 'lodash';
import { FilterPanel } from '../../Shared/Components/FilterPanel';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import {
    getIsBulkSelected,
    getIsPanelOpen,
    getIsPullTenantSelected,
    getPullTenantSearchCriteria,
    getPullTenantSearchSelection,
    getPullTenantSummaryData,
    getSelectedApprovalRecords,
    getTableRowCount,
    getTenantIdFromAppName,
    getToggleDetailsScreen,
    getPropertyFilters,
    getPropertyFilteredData,
    getUserPreferences,
} from '../../Shared/SharedComponents.selectors';
import { booleanToReadableValue, isMediumResolution, isMobileResolution } from '../../../Helpers/sharedHelpers';
import {
    requestPullTenantSummary,
    updatePullTenantSearchSelection,
    updatePropertyFilters,
    updateVisibleColumns,
    SaveUserPreferencesRequest,
} from '../../Shared/SharedComponents.actions';
import { getUserAlias, getPersistedVisibleColumns } from '../../Shared/SharedComponents.persistent-selectors';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { COLUMN_DISPLAY_NAMES, MANDATORY_COLUMNS, DEFAULT_VISIBLE_COLUMNS, TABLE_COLUMNS_DEFAULT, TABLE_COLUMNS_PULLTENANT } from '../../Shared/SharedConstants';
import { trackFeatureUsageEvent, TrackingEventId } from '../../../Helpers/telemetryHelpers';

export function SummaryTable(props: any) {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    useDynamicReducer(detailsReducerName, detailsReducer as Reducer, [detailsSagas], false);
    const { useSelector, dispatch, authClient, telemetryClient } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const isBulkSelected = useSelector(getIsBulkSelected);
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
    const tableRowCount = useSelector(getTableRowCount);
    const searchCriteria: any = useSelector(getPullTenantSearchCriteria);
    const searchSelection = useSelector(getPullTenantSearchSelection);
    const pullTenantSummaryData = useSelector(getPullTenantSummaryData);
    const tenantIdforFilterValue = useSelector((state: any) => getTenantIdFromAppName(state, props.tenantName));
    const userAlias = useSelector(getUserAlias);
    const toggleDetailsScreen = useSelector(getToggleDetailsScreen);
    const isPanelOpen = useSelector(getIsPanelOpen);
    const propertyFilters = useSelector(getPropertyFilters);
    const tenantType = props?.isPullTenant ? 'pullTenant' : 'all';
    const visibleColumns = useSelector((state: any) => getPersistedVisibleColumns(state, tenantType));
    const userPreferences = useSelector(getUserPreferences);

    const [dimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });
    const [isFilterPanelOpen, setIsFilterPanelOpen] = React.useState(false);
    const [isColumnCalloutOpen, setIsColumnCalloutOpen] = React.useState(false);
    const editColumnsButtonRef = React.useRef<HTMLDivElement>(null);
    const [searchCriteriaOptions, setSearchCriteriaOptions] = React.useState([]);
    const [searchCriteriaIconOptions, setSearchCriteriaIconOptions] = React.useState<any>([]);

    const tableColumns = props?.isPullTenant ? pullTenantTableColumns : defaultTableColumns;

    // Use selector to get filtered data instead of local state
    const filteredData = useSelector((state: any) => getPropertyFilteredData(state, props.tenantGroup, tableColumns));

    // Calculate number of active filters
    const numFilters = React.useMemo(() => {
        return Object.values(propertyFilters).reduce((count: number, values: string[]) => {
            return count + (values?.length || 0);
        }, 0);
    }, [propertyFilters]);
    const isFilterVisible = props?.tenantGroup?.length > 0 && isBulkSelected;
    const isSearchCriteriaVisible = props?.isPullTenant && searchCriteria;

    const isMaximized = isPanelOpen && toggleDetailsScreen;
    const isPanelOpen1280 = isPanelOpen && window.outerWidth <= 1280;
    const isMedium = isMediumResolution(dimensions.width);
    const isMobile = isMobileResolution(dimensions.width);

    const onSearchCriteriaChange = (
        event: React.FormEvent<HTMLDivElement> | React.MouseEvent<HTMLElement> | React.KeyboardEvent<HTMLElement>,
        item: IDropdownOption | IContextualMenuItem
    ): void => {
        const itemKey = typeof item?.key === 'string' ? parseInt(item?.key) : item.key;
        if (itemKey === 0 || itemKey !== searchSelection) {
            const filterCriteria = searchCriteria[itemKey].value;
            dispatch(requestPullTenantSummary(tenantIdforFilterValue, userAlias, filterCriteria));
            dispatch(updatePullTenantSearchSelection(itemKey));
        }
    };

    React.useEffect(() => {
        if (pullTenantSummaryData && pullTenantSummaryData.length > 0 && searchCriteria) {
            const dropdownMapping = searchCriteria.map((item: any, index: number) => {
                return { key: index, text: item.name };
            });
            setSearchCriteriaOptions(dropdownMapping);
        }

        if (
            (isMaximized || isMobile || isPanelOpen1280 || isMedium) &&
            pullTenantSummaryData &&
            pullTenantSummaryData.length > 0 &&
            searchCriteria
        ) {
            const menuItems: IContextualMenuItem[] = searchCriteria.map((item: any, index: number) => {
                return {
                    key: index.toString(),
                    text: item.name,
                    onClick: onSearchCriteriaChange,
                    canCheck: true,
                    isChecked: index === searchSelection,
                };
            });
            const menuProps: IContextualMenuProps = { items: menuItems, directionalHintFixed: true };
            setSearchCriteriaIconOptions(menuProps);
        }
    }, [searchCriteria, pullTenantSummaryData, isMaximized, isMobile, isPanelOpen1280, isMedium]);

    function getUniqueValuesForColumn(data: any[], columnKey: string, defaultValue?: string): any {
        let res = [];
        let uniqueVals: any = [];
        let selectedKeys: string[] = [];
        let i;
        for (i = 0; i < data.length; i++) {
            let curValue = columnKey.includes('.')
                ? columnKey.split('.').reduce(function (p, prop) {
                      return p?.[prop];
                  }, data[i])
                : data[i][columnKey];
            if (typeof curValue === 'boolean') {
                curValue = booleanToReadableValue(curValue);
            }
            if (defaultValue && !curValue) {
                curValue = defaultValue;
            }
            if (curValue && !uniqueVals.includes(curValue)) {
                const filterSelected = propertyFilters?.[columnKey]?.includes(curValue) ?? false;
                res.push({ label: curValue, checked: filterSelected });
                if (filterSelected) {
                    selectedKeys.push(curValue);
                }
                uniqueVals.push(curValue);
            }
        }
        if (propertyFilters?.[columnKey]) {
            for (i = 0; i < propertyFilters[columnKey].length; i++) {
                const filterElement = propertyFilters[columnKey][i];
                if (!uniqueVals.includes(filterElement)) {
                    res.push({ label: filterElement, checked: true });
                    selectedKeys.push(filterElement);
                    uniqueVals.push(filterElement);
                }
            }
        }
        return [res, selectedKeys];
    }

    function renderFilterPanel(): any {
        const filterCategories = tableColumns
            .filter((col) => col.isFilterable)
            ?.map((item, index) => {
                const [uniqueValues, selectedKeys] = getUniqueValuesForColumn(
                    props.tenantGroup,
                    item.field,
                    item.defaultValue
                );
                return {
                    key: item.field,
                    label: item.title,
                    filterOptions: uniqueValues,
                    selectedKeys: selectedKeys,
                };
            });

        return (
            <FilterPanel
                filterCategories={filterCategories}
                onChange={(
                    ev: React.FormEvent<HTMLElement>,
                    checked: boolean,
                    columnCategory: { key: string; label: string },
                    optionLabel: string
                ): void => {
                    const curSelections = propertyFilters?.[columnCategory.key] ?? [];
                    const newFilters = { ...propertyFilters };
                    if (checked && !curSelections.includes(optionLabel)) {
                        newFilters[columnCategory.key] = curSelections.concat([optionLabel]);
                    } else if (!checked && curSelections.includes(optionLabel)) {
                        newFilters[columnCategory.key] = curSelections.filter((val: string) => optionLabel !== val);
                    }
                    dispatch(updatePropertyFilters(newFilters));
                }}
                onClear={(columnCategory: { key: string; label: string }): void => {
                    const newFilters = { ...propertyFilters };
                    newFilters[columnCategory.key] = [];
                    dispatch(updatePropertyFilters(newFilters));
                }}
                isMobile={isMobile}
                onClosePanel={(): void => {
                    setIsFilterPanelOpen(false);
                }}
            />
        );
    }

    const saveTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(null);
    const userPreferencesRef = React.useRef(userPreferences);
    userPreferencesRef.current = userPreferences;

    React.useEffect(() => {
        return () => {
            if (saveTimerRef.current) {
                clearTimeout(saveTimerRef.current);
            }
        };
    }, []);

    function saveColumnPreference(columns: string[]): void {
        // Update Redux state immediately
        dispatch(updateVisibleColumns(tenantType, columns));

        // Debounce API save (500ms) to batch rapid toggles
        if (saveTimerRef.current) {
            clearTimeout(saveTimerRef.current);
        }
        saveTimerRef.current = setTimeout(() => {
            const prefKey = tenantType === 'pullTenant' ? TABLE_COLUMNS_PULLTENANT : TABLE_COLUMNS_DEFAULT;
            const prefs = [...(userPreferencesRef.current || [])];
            const existing = prefs.findIndex((p: any) => p.UserPreferenceText === prefKey);
            if (existing >= 0) {
                prefs[existing] = { ...prefs[existing], UserPreferenceStatus: JSON.stringify(columns) };
            } else {
                prefs.push({ UserPreferenceText: prefKey, UserPreferenceStatus: JSON.stringify(columns) });
            }
            dispatch(SaveUserPreferencesRequest({ FeaturePreferenceJson: JSON.stringify(prefs) }, true));
        }, 500);
    }

    function renderColumnEditor(): JSX.Element {
        const defaultKey = props?.isPullTenant ? 'PullTenant' : 'Default';
        const allAvailableColumns = DEFAULT_VISIBLE_COLUMNS[defaultKey] || [];
        const mandatoryCols = MANDATORY_COLUMNS[defaultKey] || [];

        return (
            <Stack tokens={{ childrenGap: 8 }}>
                <Stack.Item>
                    <strong>Edit Columns</strong>
                </Stack.Item>
                {allAvailableColumns.map((fieldName: string) => {
                    const isMandatory = mandatoryCols.includes(fieldName);
                    const isChecked = visibleColumns.includes(fieldName);
                    return (
                        <Stack.Item key={fieldName}>
                            <Checkbox
                                label={COLUMN_DISPLAY_NAMES[fieldName] || fieldName}
                                checked={isChecked}
                                disabled={isMandatory}
                                onChange={(_ev, checked): void => {
                                    const isChecked = !!checked;
                                    const newVisible = isChecked
                                        ? allAvailableColumns.filter(
                                              (c: string) => visibleColumns.includes(c) || c === fieldName
                                          )
                                        : visibleColumns.filter((c: string) => c !== fieldName);
                                    saveColumnPreference(newVisible);
                                    trackFeatureUsageEvent(
                                        authClient,
                                        telemetryClient,
                                        `ColumnToggle - ${isChecked ? 'Show' : 'Hide'} ${COLUMN_DISPLAY_NAMES[fieldName] || fieldName}`,
                                        'MSApprovals.ColumnToggle',
                                        TrackingEventId.ColumnToggle,
                                        null,
                                        { Column: fieldName, Visible: String(isChecked), TenantType: tenantType }
                                    );
                                }}
                            />
                        </Stack.Item>
                    );
                })}
                <Stack.Item styles={{ root: { marginTop: 8 } }}>
                    <DefaultButton
                        text="Reset to defaults"
                        onClick={(): void => {
                            saveColumnPreference(allAvailableColumns);
                            trackFeatureUsageEvent(
                                authClient,
                                telemetryClient,
                                'ColumnResetDefaults',
                                'MSApprovals.ColumnResetDefaults',
                                TrackingEventId.ColumnResetDefaults,
                                null,
                                { TenantType: tenantType, TenantName: props.tenantName }
                            );
                        }}
                        styles={{ root: { width: '100%' } }}
                    />
                </Stack.Item>
            </Stack>
        );
    }

    function renderDataGrid(): JSX.Element {
        const filterGrid = (
            <Styled.DataGridContainer isFilterPanelOpen={isFilterPanelOpen}>
                <SummaryTableColumns
                    tenantGroup={filteredData}
                    tenantName={props.tenantName}
                    numFilters={numFilters}
                    isSingleGroupShown={props.isSingleGroupShown}
                    isPullTenant={props.isPullTenant}
                />
                {isFilterPanelOpen && renderFilterPanel()}
            </Styled.DataGridContainer>
        );
        const normalGrid = (
            <SummaryTableColumns
                tenantGroup={props.tenantGroup}
                tenantName={props.tenantName}
                numFilters={0}
                isSingleGroupShown={props.isSingleGroupShown}
                isPullTenant={props.isPullTenant}
            />
        );
        const grid = isFilterVisible ? filterGrid : normalGrid;
        return grid;
    }

    const renderRowCount = (): JSX.Element => {
        return (
            <Stack.Item align="end" styles={Styled.StackStylesRowCount(isMaximized, isPanelOpen)}>
                <span>
                    Total rows: <strong>{tableRowCount}</strong>
                </span>
                <span>{' | '}</span>
                <span>
                    Selected rows: <strong>{selectedApprovalRecords.length}</strong>
                </span>
            </Stack.Item>
        );
    };

    return (
        <SummaryStyled.SummaryTablesContainer isBulkSelected={isBulkSelected}>
            <div className="ms-Grid" dir="ltr">
                <div>
                    <Styled.SummaryTableContainer
                        isDashboardView={props.isDashboardView}
                        style={{
                            height: `${props.windowHeight - 300}px}`,
                        }}
                    >
                        <Stack horizontal horizontalAlign="space-between" styles={{ root: { marginBottom: '10px' } }}>
                            {isBulkSelected && renderRowCount()}
                            <Stack.Item>
                                <Stack horizontal tokens={{ childrenGap: 8 }}>
                                    {isFilterVisible && numFilters > 0 && !props?.isPullTenant && (
                                        <Stack.Item>
                                            <FilterTags
                                                tags={Object.entries(propertyFilters).flatMap(([key, values]) => {
                                                    const displayName =
                                                        tenantColumns[key]?.displayName ||
                                                        key.replace(/([A-Z])/g, ' $1').trim();
                                                    return (values || []).map((value: string) => ({
                                                        key: `${key}:${value}`,
                                                        label: `${displayName}: ${value}`,
                                                    }));
                                                })}
                                                onRemoveTag={(tagKey: string) => {
                                                    const [key, value] = tagKey.split(':');
                                                    const newFilters = { ...propertyFilters };
                                                    newFilters[key] = (propertyFilters[key] || []).filter(
                                                        (v: string) => v !== value
                                                    );
                                                    dispatch(updatePropertyFilters(newFilters));
                                                }}
                                                showLabel={false}
                                            />
                                        </Stack.Item>
                                    )}
                                    {isSearchCriteriaVisible && (
                                        <Stack.Item>
                                            {isMaximized || isMobile || isPanelOpen1280 || isMedium ? (
                                                <IconButton
                                                    id="searchCriteria"
                                                    iconProps={{ iconName: 'Search' }}
                                                    title="Search Criteria"
                                                    ariaLabel="Search Criteria"
                                                    menuProps={searchCriteriaIconOptions}
                                                />
                                            ) : (
                                                <Dropdown
                                                    onChange={onSearchCriteriaChange}
                                                    options={searchCriteriaOptions}
                                                    selectedKey={searchSelection}
                                                    label="Search Criteria"
                                                    styles={Styled.searchCriteraDropdownStyles}
                                                    placeholder="Select a range"
                                                />
                                            )}
                                        </Stack.Item>
                                    )}
                                    {isFilterVisible && (
                                        <Stack.Item>
                                            <IconButton
                                                iconProps={{ iconName: numFilters > 0 ? 'FilterSolid' : 'Filter' }}
                                                title="Filter data grid"
                                                ariaLabel="Filter data grid"
                                                onClick={(): void => {
                                                    setIsFilterPanelOpen(!isFilterPanelOpen);
                                                }}
                                            />
                                        </Stack.Item>
                                    )}
                                    {isFilterVisible && (
                                        <Stack.Item>
                                            <div ref={editColumnsButtonRef}>
                                                <IconButton
                                                    iconProps={{ iconName: 'ColumnOptions' }}
                                                    title="Edit Columns"
                                                    ariaLabel="Edit Columns"
                                                    onClick={(): void => {
                                                        const opening = !isColumnCalloutOpen;
                                                        setIsColumnCalloutOpen(opening);
                                                        trackFeatureUsageEvent(
                                                            authClient,
                                                            telemetryClient,
                                                            opening ? 'EditColumns - Open' : 'EditColumns - Close',
                                                            'MSApprovals.EditColumns',
                                                            opening ? TrackingEventId.EditColumnsOpen : TrackingEventId.EditColumnsClose,
                                                            null,
                                                            { TenantType: tenantType, TenantName: props.tenantName }
                                                        );
                                                    }}
                                                />
                                            </div>
                                        </Stack.Item>
                                    )}
                                </Stack>
                            </Stack.Item>
                        </Stack>

                        {isColumnCalloutOpen && editColumnsButtonRef.current && (
                            <Callout
                                target={editColumnsButtonRef.current}
                                onDismiss={(): void => setIsColumnCalloutOpen(false)}
                                directionalHint={DirectionalHint.bottomLeftEdge}
                                gapSpace={4}
                                styles={{ root: { padding: '16px', minWidth: 220 } }}
                            >
                                {renderColumnEditor()}
                            </Callout>
                        )}

                        <Stack.Item>
                            <Styled.SummaryTableMainContainer
                                style={{
                                    height: `${
                                        dimensions.width < 1024 ? dimensions.height - 280 : dimensions.height - 490
                                    }px}`,
                                }}
                            >
                                {renderDataGrid()}
                            </Styled.SummaryTableMainContainer>
                        </Stack.Item>
                    </Styled.SummaryTableContainer>
                </div>
            </div>
        </SummaryStyled.SummaryTablesContainer>
    );
}
