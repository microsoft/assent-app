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
    Dropdown,
    IconButton,
    IContextualMenuItem,
    IContextualMenuProps,
    IDropdownOption
} from '@fluentui/react';
import { tableColumns } from './PullTenantColumns';
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
    getToggleDetailsScreen
} from '../../Shared/SharedComponents.selectors';
import { booleanToReadableValue, isMobileResolution } from '../../../Helpers/sharedHelpers';
import { requestPullTenantSummary, updatePullTenantSearchSelection } from '../../Shared/SharedComponents.actions';
import { getUserAlias } from '../../Shared/SharedComponents.persistent-selectors';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

export function SummaryTable(props: any) {
    useDynamicReducer(sharedComponentsReducerName, sharedComponentsReducer as Reducer, [sharedComponentsSagas], false);
    useDynamicReducer(detailsReducerName, detailsReducer as Reducer, [detailsSagas], false);
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

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

    const [dimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth
    });
    const [isFilterPanelOpen, setIsFilterPanelOpen] = React.useState(false);
    const [includedValuesObj, setIncludedValuesObj] = React.useState(undefined);
    const [filteredData, setFilteredData] = React.useState(props.tenantGroup);
    const [numFilters, setNumFilters] = React.useState(0);
    const [searchCriteriaOptions, setSearchCriteriaOptions] = React.useState([]);
    const [searchCriteriaIconOptions, setSearchCriteriaIconOptions] = React.useState<any>([]);

    const isFilterVisible = props?.isPullTenant && props?.tenantGroup?.length > 0;
    const isSearchCriteriaVisible = props?.isPullTenant && searchCriteria;

    const isMaximized = isPanelOpen && toggleDetailsScreen;

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
        setFilteredData(props.tenantGroup);
    }, [props.tenantGroup]);

    React.useEffect(() => {
        if (pullTenantSummaryData && pullTenantSummaryData.length > 0 && searchCriteria) {
            const dropdownMapping = searchCriteria.map((item: any, index: number) => {
                return { key: index, text: item.name };
            });
            setSearchCriteriaOptions(dropdownMapping);
        }
        if ((isMaximized || isMobile) && pullTenantSummaryData && pullTenantSummaryData.length > 0 && searchCriteria) {
            const menuItems: IContextualMenuItem[] = searchCriteria.map((item: any, index: number) => {
                return {
                    key: index.toString(),
                    text: item.name,
                    onClick: onSearchCriteriaChange,
                    canCheck: true,
                    isChecked: index === searchSelection
                };
            });
            const menuProps: IContextualMenuProps = { items: menuItems, directionalHintFixed: true };
            setSearchCriteriaIconOptions(menuProps);
        }
    }, [searchCriteria, pullTenantSummaryData, isMaximized, isMobile]);

    React.useEffect(() => {
        const filtered = numFilters > 0 ? filterTable(props.tenantGroup, includedValuesObj) : props.tenantGroup;
        setFilteredData(filtered);
    }, [includedValuesObj, numFilters, props.tenantGroup]);

    function getUniqueValuesForColumn(data: any[], columnKey: string): any[] {
        let res = [];
        let uniqueVals: any = [];
        let i;
        for (i = 0; i < data.length; i++) {
            let curValue = columnKey.includes('.')
                ? columnKey.split('.').reduce(function(p, prop) {
                      return p?.[prop];
                  }, data[i])
                : data[i][columnKey];
            if (typeof curValue === 'boolean') {
                curValue = booleanToReadableValue(curValue);
            }
            if (curValue && !uniqueVals.includes(curValue)) {
                const filterSelected = includedValuesObj?.[columnKey]?.includes(curValue) ?? false;
                res.push({ label: curValue, checked: filterSelected });
                uniqueVals.push(curValue);
            }
        }
        if (includedValuesObj?.[columnKey]) {
            for (i = 0; i < includedValuesObj[columnKey].length; i++) {
                const filterElement = includedValuesObj[columnKey][i];
                if (!uniqueVals.includes(filterElement)) {
                    res.push({ label: filterElement, checked: true });
                    uniqueVals.push(filterElement);
                }
            }
        }
        return res;
    }

    function filterTable(data: any[], includedValuesObj: object): any[] {
        return data.filter(item => {
            let res = true;
            forEach(includedValuesObj, function(value: any, key: any) {
                let curValue = key.includes('.')
                    ? key.split('.').reduce(function(p: any, prop: any) {
                          return p?.[prop];
                      }, item)
                    : item[key];
                if (typeof curValue === 'boolean') {
                    curValue = booleanToReadableValue(curValue);
                }
                if (value && value.length > 0 && !value.includes(curValue)) {
                    res = false;
                }
            });
            return res;
        });
    }

    function renderFilterPanel(): any {
        const filterCategories = tableColumns
            .filter(col => col.isFilterable)
            ?.map((item, index) => {
                const uniqueValues = getUniqueValuesForColumn(props.tenantGroup, item.field);
                return {
                    key: item.field,
                    label: item.title,
                    filterOptions: uniqueValues
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
                    const curSelections = includedValuesObj[columnCategory.key] ?? [];
                    let newSelectionObj: any = {};
                    if (checked && !curSelections.includes(optionLabel)) {
                        newSelectionObj[columnCategory.key] = curSelections.concat([optionLabel]);
                        setNumFilters(numFilters + 1);
                        setIncludedValuesObj((prevState: any) => {
                            return { ...prevState, ...newSelectionObj };
                        });
                    } else if (!checked && curSelections.includes(optionLabel)) {
                        newSelectionObj[columnCategory.key] = curSelections.filter(
                            (val: string) => optionLabel !== val
                        );
                        setNumFilters(numFilters - 1);
                        setIncludedValuesObj((prevState: any) => {
                            return { ...prevState, ...newSelectionObj };
                        });
                    }
                }}
                onClear={(columnCategory: { key: string; label: string }): void => {
                    const curSelections = includedValuesObj[columnCategory.key] ?? [];
                    const numRemoved = curSelections.length;
                    let newSelectionObj: any = {};
                    newSelectionObj[columnCategory.key] = [];
                    setNumFilters(numFilters - numRemoved);
                    setIncludedValuesObj((prevState: any) => {
                        return { ...prevState, ...newSelectionObj };
                    });
                }}
                isMobile={isMobile}
                onClosePanel={(): void => {
                    setIsFilterPanelOpen(false);
                }}
            />
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
                        style={{
                            height: `${props.windowHeight - 300}px}`
                        }}
                    >
                        <Stack horizontal horizontalAlign="space-between" styles={{ root: { marginBottom: '10px' } }}>
                            {isBulkSelected && renderRowCount()}
                            <Stack.Item>
                                <Stack horizontal>
                                    {isSearchCriteriaVisible && (
                                        <Stack.Item>
                                            {isMaximized || isMobile ? (
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
                                </Stack>
                            </Stack.Item>
                        </Stack>

                        <Stack.Item>
                            <Styled.SummaryTableMainContainer
                                style={{
                                    height: `${
                                        dimensions.width < 1024 ? dimensions.height - 280 : dimensions.height - 490
                                    }px}`
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
