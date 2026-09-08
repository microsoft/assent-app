/* eslint-disable react/display-name */
import * as React from 'react';
import * as Styled from './SummaryTableStyling';
import * as SharedStyled from '../../Shared/SharedLayout';
import {
    failedIconStyle,
    pendingIconStyle,
    fileIconCell,
    paginationWidth,
    paginationAlign,
} from './SummaryTableStyling';
import { ISummaryRecordModel } from './SummaryTable.types';
import { SummaryTableFieldNames } from './SummaryTableFieldNames';
import {
    booleanToReadableValue,
    flattenObject,
    imitateClickOnKeyPressForAnchor,
    validateBulkCondition,
    formatUnitValue,
} from '../../../Helpers/sharedHelpers';
import { Dictionary } from 'adaptivecards';
import {
    TooltipHost,
    IDetailsListProps,
    DetailsHeader,
    IContextualMenuProps,
    Dialog,
    TextField,
    DialogFooter,
    PrimaryButton,
    DefaultButton,
    ContextualMenu,
    DialogType,
    ITextField,
} from '@fluentui/react';
import {
    updatePanelState,
    updateRetainBulkSelection,
    updateTableRowCount,
    updateVisibleColumns,
} from '../../Shared/SharedComponents.actions';
import { updateMyRequest } from '../../Shared/Details/Details.actions';
import { Stack } from '@fluentui/react/lib/Stack';
import { useActiveElement } from '../../Shared/Components/ActiveElement';
import { CheckboxVisibility } from '@fluentui/react';
import {
    DetailsListLayoutMode,
    DetailsRow,
    IDetailsRowStyles,
    Selection,
    SelectionMode,
    DetailsList,
    IColumn,
    IDetailsList,
} from '@fluentui/react/lib/DetailsList';
import { Icon } from '@fluentui/react/lib/Icon';
import { updateApprovalRecords } from '../../Shared/SharedComponents.actions';
import { SubmitterPersona } from '../../Shared/Components/SubmitterPersona';
import { PersonaSize } from '../../Shared/Components/Persona/Persona.types';
import { TextColors } from '../../Shared/SharedColors';
import {
    getBulkActionConcurrentCall,
    getBulkFailedMsg,
    getFailedPullTenantRequests,
    getFilteredTenantInfo,
    getIsBulkSelected,
    getIsBulkSelectionRetained,
    getIsLoadingSummary,
    getIsPaginationEnabled,
    getSelectedApprovalRecords,
    getTableRowCount,
} from '../../Shared/SharedComponents.selectors';
import { getPersistedVisibleColumns } from '../../Shared/SharedComponents.persistent-selectors';
import {
    getDisplayDocumentNumber,
    getFailedRequests,
    getIsDisabled,
    getPostActionErrorMessage,
    getReadRequests,
} from '../../Shared/Details/Details.selectors';
import { breakpointMap } from '../../Shared/Styles/Media';
import { tableColumns } from './PullTenantColumns';
import { DATE_FORMAT_OPTION, DEFAULT_LOCALE } from '../../Shared/SharedConstants';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { Link } from '../../Shared/Styles/Link';
import { getIsKeyboardColumnResizingOn } from '../../AccessibilityPanel/Accessibility.selectors';
import { useHistory } from 'react-router-dom';
import { NavigationUtils } from '../../Shared/Utils/NavigationUtils';

const ColSize = {
    XXS: 40,
    XS: 70,
    Small: 80,
    Medium: 90,
    Large: 110,
    XL: 120,
    XXL: 200,
    XXXL: 230,
};

const RESIZE = 'Resize';

const COLUMN_HEADER_ACCESSIBILITY_LABEL = ' - Click to resize or sort column';

function SummaryTableColumns(props: any): React.ReactElement {
    const { useSelector, dispatch, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );

    const isBulkSelected = useSelector(getIsBulkSelected);
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
    const readRequests = useSelector(getReadRequests);
    const displayDocumentNumber = useSelector(getDisplayDocumentNumber);
    const bulkActionConcurrentCall = useSelector(getBulkActionConcurrentCall);
    const postActionErrorMessage = useSelector(getPostActionErrorMessage);
    const failedRequests = useSelector(getFailedRequests);
    const isPaginationEnabled = useSelector(getIsPaginationEnabled);
    // Use prop instead of global selector to avoid tenant mismatch when
    // filterValue resets during preference saves.
    const isPullTenantSelected = props.isPullTenant === true;
    const failedPullTenantRequests = useSelector(getFailedPullTenantRequests);
    const filteredTenantInfo = useSelector(getFilteredTenantInfo);
    const tableRowCount = useSelector(getTableRowCount);
    const disabled = useSelector(getIsDisabled);
    const isKeyboardColumnResizingOn = useSelector(getIsKeyboardColumnResizingOn);
    const visibleColumnsDefault = useSelector((state: any) => getPersistedVisibleColumns(state, 'all'));
    const visibleColumnsPullTenant = useSelector((state: any) => getPersistedVisibleColumns(state, 'pullTenant'));

    const history = useHistory();

    const [windowWidth, setWindowWidth] = React.useState<number>(0);
    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });
    const focusedElement = useActiveElement();
    const [rowsPerPage, setRowsPerPage] = React.useState<number>(5);
    const [pageCount, setPageCount] = React.useState<number>(Math.ceil(props.tenantGroup.length / rowsPerPage));
    const [selectedPage, setSelectedPage] = React.useState<number>(1);
    const [summaryTableRecords, setSummaryTableRecords] = React.useState(props.tenantGroup);
    const [selectedRowState, setSelectedRowState] = React.useState([]);
    const [selectedRowStateObj, setSelectedRowStateObj] = React.useState(undefined);
    const [finalRowSelected, setFinalRowSelected] = React.useState([]);
    const [isAllChecked, setIsAllChecked] = React.useState(false);
    const [areItemsUpdated, setAreItemsUpdated] = React.useState(false);
    const [isSelectAllConfigured, setIsSelectAllConfigured] = React.useState(false);
    const [savedSelection, setSavedSelection] = React.useState([]);
    const [numFilters, setNumFilters] = React.useState(0);
    const [sortedColumn, setSortedColumn] = React.useState('');
    const [isSortedDescending, setIsSortedDescending] = React.useState(false);

    let detailsListRef = React.useRef<IDetailsList>(null);
    const input = React.useRef<number | null>(null);
    const [isDialogHidden, setIsDialogHidden] = React.useState(true);
    const textfieldRef = React.useRef<ITextField>(null);
    const columnToEdit = React.useRef<IColumn | null>(null);
    const clickHandler = React.useRef<string>(RESIZE);
    const [contextualMenuProps, setContextualMenuProps] = React.useState<IContextualMenuProps | undefined>(undefined);

    const documentNumberPrefix = filteredTenantInfo?.documentNumberPrefix;
    const isSingleGroupShown = props?.isSingleGroupShown ?? false;
    const filteredTenantId = filteredTenantInfo?.tenantId;

    const setTableRecords = (records: object[]): void => {
        setSummaryTableRecords(records);
        if (isBulkSelected && records.length !== tableRowCount) {
            dispatch(updateTableRowCount(records.length));
        }
    };

    const getCanSelectItem = (item: any): boolean => {
        let conditionPropertyObject = {};
        if (isPullTenantSelected) {
            return item['allowInBulkApproval'] && (item?.isSelectable ?? true);
        } else {
            conditionPropertyObject = flattenObject(item);
            const isBulkConditionValid = item?.AllowBulkApprovalCondition
                ? validateBulkCondition(conditionPropertyObject, item?.AllowBulkApprovalCondition)
                : true;
            const res = item['IsControlsAndComplianceRequired']
                ? item['IsRead'] && (item?.isSelectable ?? true)
                : item?.isSelectable ?? true;
            return res && isBulkConditionValid;
        }
    };

    const getIdentifyingKey = (item: any): string => {
        if (isPullTenantSelected) {
            return item['laborId'];
        } else {
            return item.ApprovalIdentifier?.DisplayDocumentNumber;
        }
    };

    const getKeyForSelectedRecords = (item: any): string => {
        if (isPullTenantSelected) {
            return item['laborId'];
        } else {
            return item.DisplayDocumentNumber;
        }
    };

    const getIsCompliant = (item: any): boolean => {
        if (isPullTenantSelected) {
            return item['allowInBulkApproval'];
        } else {
            return item['IsControlsAndComplianceRequired'] ? item['IsRead'] : true;
        }
    };
    const isBulkSelectionRetained = useSelector(getIsBulkSelectionRetained);

    // DataGrid Selection Functionality Controlled here.
    const selection = new Selection({
        canSelectItem: getCanSelectItem,
        getKey: (item: any): string => {
            return item.ApprovalIdentifier?.DisplayDocumentNumber || item['laborId'];
        },
        onSelectionChanged: () => {
            setSelectedRowState(selectionState.getSelection());
            setIsAllChecked(selectionState.isAllSelected());
        },
    });
    const [selectionState, setSelectionState] = React.useState(selection);

    function handleMaxSelection(curSelected: object[], allItems: object[]): object[] {
        let i;
        const numSelected = curSelected.length;
        let numRemaining = bulkActionConcurrentCall - numSelected;
        let newItems = [];
        for (i = 0; i < allItems.length; i++) {
            const curItem: any = allItems[i];
            const isCompliant = getIsCompliant(curItem);
            const isUserSelected = curSelected.some((e) => getIdentifyingKey(e) === getIdentifyingKey(curItem));
            let isSelectable = isUserSelected;
            if (numRemaining > 0 && isCompliant && !isUserSelected) {
                isSelectable = true;
                numRemaining = numRemaining - 1;
            }
            const isRead = curItem['IsRead'] || readRequests?.includes(getIdentifyingKey(curItem));
            newItems[i] = { ...curItem, isSelectable: isSelectable, IsRead: isRead };
        }
        return newItems;
    }

    function clearMaxSelection(allItems: object[]): object[] {
        let i;
        let newItems = [];
        for (i = 0; i < allItems.length; i++) {
            const curItem = allItems[i];
            const isSelectable = true;
            newItems[i] = { ...curItem, isSelectable: isSelectable };
        }
        return newItems;
    }

    // Helper function to sort items by column key
    function sortItems(items: any[], columnKey: string, descending: boolean): any[] {
        if (!columnKey || items.length === 0) {
            return items;
        }

        const sortedItems = [...items];
        const column = allColumns.find((col) => col.key === columnKey);

        if (!column) {
            return items;
        }

        switch (columnKey) {
            case 'ApprovalIdentifier':
                sortedItems.sort((a: any, b: any) => {
                    const comparison =
                        a.ApprovalIdentifier?.DisplayDocumentNumber?.localeCompare(
                            b.ApprovalIdentifier?.DisplayDocumentNumber || '',
                            'en-US',
                            { numeric: true, sensitivity: 'base' }
                        ) || 0;
                    return descending ? -comparison : comparison;
                });
                break;
            case 'Submitter':
                sortedItems.sort((a: any, b: any) => {
                    const aName = a.Submitter?.Name || '';
                    const bName = b.Submitter?.Name || '';
                    const comparison = aName > bName ? 1 : aName < bName ? -1 : 0;
                    return descending ? -comparison : comparison;
                });
                break;
            case 'UnitValue':
                sortedItems.sort((a: any, b: any) => {
                    const comparison = (a.UnitValue || '').localeCompare(b.UnitValue || '', 'en-US', {
                        numeric: true,
                        sensitivity: 'base',
                    });
                    return descending ? -comparison : comparison;
                });
                break;
            case 'SubmittedDate':
            case 'laborDate':
                sortedItems.sort((a: any, b: any) => {
                    const aTime = a[columnKey] ? new Date(a[columnKey]).getTime() : 0;
                    const bTime = b[columnKey] ? new Date(b[columnKey]).getTime() : 0;
                    const comparison = aTime - bTime;
                    return descending ? -comparison : comparison;
                });
                break;
            case 'displayLaborHours':
            case 'laborHours':
                sortedItems.sort((a: any, b: any) => {
                    const comparison = (parseFloat(a.laborHours) || 0) - (parseFloat(b.laborHours) || 0);
                    return descending ? -comparison : comparison;
                });
                break;
            case 'actionDetails':
                sortedItems.sort((a: any, b: any) => {
                    const aType = a.actionDetails?.actionType || '';
                    const bType = b.actionDetails?.actionType || '';
                    const comparison = aType > bType ? 1 : aType < bType ? -1 : 0;
                    return descending ? -comparison : comparison;
                });
                break;
            case 'assignmentName':
                sortedItems.sort((a: any, b: any) => {
                    const aName = a.assignmentDetails?.assignmentName || '';
                    const bName = b.assignmentDetails?.assignmentName || '';
                    const comparison = aName > bName ? 1 : aName < bName ? -1 : 0;
                    return descending ? -comparison : comparison;
                });
                break;
            case 'isBillable':
                sortedItems.sort((a: any, b: any) => {
                    const aBillable = a.assignmentDetails?.isBillable ?? false;
                    const bBillable = b.assignmentDetails?.isBillable ?? false;
                    const comparison = aBillable > bBillable ? 1 : aBillable < bBillable ? -1 : 0;
                    return descending ? -comparison : comparison;
                });
                break;
            case 'allowInBulkApproval':
                sortedItems.sort((a: any, b: any) => {
                    const aType = a?.actionDetails?.[0]?.actionType ?? 'Standard';
                    const bType = b?.actionDetails?.[0]?.actionType ?? 'Standard';
                    const comparison = aType > bType ? 1 : aType < bType ? -1 : 0;
                    return descending ? -comparison : comparison;
                });
                break;
            case 'isRead':
            case 'IsRead':
                sortedItems.sort((a: any, b: any) => {
                    const comparison = (a.IsRead ? 1 : 0) - (b.IsRead ? 1 : 0);
                    return descending ? -comparison : comparison;
                });
                break;
            default:
                // Generic comparison for other columns
                sortedItems.sort((a: any, b: any) => {
                    let aValue, bValue;
                    const fieldName = column.fieldName;

                    if (fieldName && fieldName.includes('.')) {
                        aValue = fieldName.split('.').reduce((obj, key) => obj?.[key], a) || '';
                        bValue = fieldName.split('.').reduce((obj, key) => obj?.[key], b) || '';
                    } else {
                        aValue = a[columnKey] || '';
                        bValue = b[columnKey] || '';
                    }

                    const comparison =
                        typeof aValue === 'string' && typeof bValue === 'string'
                            ? aValue.localeCompare(bValue, undefined, { numeric: true, sensitivity: 'base' })
                            : aValue > bValue
                            ? 1
                            : aValue < bValue
                            ? -1
                            : 0;

                    return descending ? -comparison : comparison;
                });
                break;
        }

        return sortedItems;
    }

    // Reapply the current sort state (column and direction) to a set of items
    function reapplyCurrentSort(items: any[]): any[] {
        return sortItems(items, sortedColumn, isSortedDescending);
    }

    function handleResize() {
        setWindowWidth(window.innerWidth);
        setDimensions({
            height: window.innerHeight,
            width: window.innerWidth,
        });
    }

    React.useEffect(() => {
        handleResize();
        window.addEventListener('resize', handleResize);

        if (isPullTenantSelected) {
            dispatch(updateTableRowCount(props.tenantGroup.length));
        }
        if (!isBulkSelectionRetained) {
            selectionState.setAllSelected(false);
        }
        if (isBulkSelectionRetained && selectedApprovalRecords.length > 0) {
            let i;
            for (i = 0; i < selectedApprovalRecords.length; i++) {
                const curItem = selectedApprovalRecords[i];
                selectionState.setKeySelected(getKeyForSelectedRecords(curItem), true, false);
            }
            dispatch(updateRetainBulkSelection(false));
        }
        return (): void => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    React.useEffect(() => {
        if (isAllChecked && !areItemsUpdated && !isSelectAllConfigured) {
            const newItems = handleMaxSelection(selectedRowStateObj[selectedPage], selectionState.getItems());
            const sortedNewItems = reapplyCurrentSort(newItems);
            selectionState.setItems(sortedNewItems);
            setTableRecords(sortedNewItems);
            setAreItemsUpdated(true);
        } else if (!isAllChecked && areItemsUpdated && !isSelectAllConfigured) {
            selectionState.setAllSelected(true);
            setIsSelectAllConfigured(true);
        } else if (!isAllChecked && areItemsUpdated && isSelectAllConfigured) {
            const clearedItems = clearMaxSelection(selectionState.getItems());
            const sortedClearedItems = reapplyCurrentSort(clearedItems);
            setSavedSelection(selectionState.getSelection());
            selectionState.setItems(sortedClearedItems);
            setTableRecords(sortedClearedItems);
            setAreItemsUpdated(false);
            setIsSelectAllConfigured(false);
        } else if (!isAllChecked && !areItemsUpdated && !isSelectAllConfigured) {
            if (savedSelection.length > 0) {
                let i = 0;
                for (i = 0; i < savedSelection.length; i++) {
                    const curItem = savedSelection[i];
                    selectionState.setKeySelected(getIdentifyingKey(curItem), true, false);
                }
                setSavedSelection([]);
            }
        }
    }, [
        areItemsUpdated,
        isAllChecked,
        selectionState,
        isSelectAllConfigured,
        savedSelection,
        selectedRowStateObj,
        selectedPage,
    ]);

    React.useEffect(() => {
        if (!isBulkSelected) {
            setRowsPerPage(5);
        } else {
            setRowsPerPage(100);
        }
    }, [isBulkSelected]);

    React.useEffect(() => {
        if (isPaginationEnabled) {
            let records = [];
            setPageCount(Math.ceil(props.tenantGroup.length / rowsPerPage));
            records = props.tenantGroup.slice(0, rowsPerPage);
            const sortedRecords = reapplyCurrentSort(records);
            setTableRecords(sortedRecords);
        }
    }, [rowsPerPage, isPaginationEnabled]);

    React.useEffect(() => {
        // to avoid duplicates, storing with reference to selectedPage
        if (isBulkSelected) {
            let toSelectUniqueRecords: any = {};
            toSelectUniqueRecords[selectedPage] = selectionState.getSelection();
            setSelectedRowStateObj((prevState: any) => {
                return { ...prevState, ...toSelectUniqueRecords };
            });
        }
    }, [selectedRowState, isBulkSelected]);

    React.useEffect(() => {
        // to gather all selected rows and store in finalRowSelected
        // which is used to updateApprovalRecords and also update checkboxe selection after pagination
        if (isBulkSelected) {
            let allSelectedRecords = [];
            for (const key in selectedRowStateObj) {
                for (const recordIndex in selectedRowStateObj[key]) {
                    if (isPullTenantSelected) {
                        allSelectedRecords.push(selectedRowStateObj[key][recordIndex]);
                    } else {
                        allSelectedRecords.push(selectedRowStateObj[key][recordIndex].ApprovalIdentifier);
                    }
                }
            }
            setFinalRowSelected(allSelectedRecords);
            dispatch(updateApprovalRecords(allSelectedRecords));
        }
    }, [selectedRowStateObj, isBulkSelected]);

    React.useEffect(() => {
        // to trigger when we change page
        const updatedSelection = selectionState;
        if (finalRowSelected.length > 0) {
            finalRowSelected.forEach((item) => {
                updatedSelection.setKeySelected(item.DisplayDocumentNumber, true, true);
            });
        }
        setSelectionState(updatedSelection);
    }, [selectedPage]);

    const getMaxSelectable = (allRecords: object[]): number => {
        let res = 0;
        for (let i = 0; i < allRecords.length && i < bulkActionConcurrentCall; i++) {
            const item = allRecords[i];
            if (getIsCompliant(item)) {
                res++;
            }
        }
        return res;
    };

    React.useEffect(() => {
        // to locally read and select
        const maxSelectable = getMaxSelectable(summaryTableRecords);
        const isMax = selectedApprovalRecords.length >= maxSelectable;
        let temp = summaryTableRecords.map((item: any, index: number) => {
            if (readRequests.includes(getIdentifyingKey(item))) {
                item['IsRead'] = true;
                if (!isMax) {
                    item.isSelectable = true;
                }
                return item;
            }
            return item;
        });
        const lastRead = readRequests[readRequests.length - 1];
        const sortedTemp = reapplyCurrentSort(temp);
        setTableRecords(sortedTemp);
        if (!isMax && isAllChecked && areItemsUpdated && isSelectAllConfigured) {
            //prevents requests from getting auto-selected if they are opened when select all is checked
            const lastSelection = [...selectedRowState];
            selectionState.setAllSelected(false);
            for (let i = 0; i < lastSelection.length; i++) {
                const curItemKey = getIdentifyingKey(lastSelection[i]);
                if (curItemKey !== lastRead) {
                    selectionState.setKeySelected(curItemKey, true, true);
                }
            }
        }
    }, [readRequests]);

    React.useEffect(() => {
        if (isAllChecked) {
            const withSelection = handleMaxSelection(selectedRowStateObj[selectedPage], props.tenantGroup);
            const sortedWithSelection = reapplyCurrentSort(withSelection);
            setTableRecords(sortedWithSelection);
        } else {
            const sortedTenantGroup = reapplyCurrentSort(props.tenantGroup);
            setTableRecords(sortedTenantGroup);
        }
    }, [props.tenantGroup]);

    React.useEffect(() => {
        if (isAllChecked && props.numFilters < numFilters) {
            selectionState.setAllSelected(false);
        }
        setNumFilters(props.numFilters);
    }, [props.numFilters, isAllChecked]);

    const handleDocNumClicked = (tenantId: string, jsonData: any): void => {
        let docNum = jsonData.DocumentNumber;
        let displayDocNum = jsonData.DisplayDocumentNumber;
        let fiscalYear = jsonData.FiscalYear;
        if (displayDocNum !== displayDocumentNumber) {
            NavigationUtils.navigateToDetails(history, tenantId, displayDocNum, true, true);
        }
    };

    const handleLaborClicked = (laborItem: any): void => {
        if (laborItem.laborId !== displayDocumentNumber) {
            dispatch(updateMyRequest(filteredTenantId, laborItem.laborId, laborItem.laborId, '', laborItem));
            dispatch(updatePanelState(true));
        }
    };

    const isFailedRequest = (identifyingKey: string): boolean => {
        return failedRequests.includes(identifyingKey);
    };

    const isFailedBulkRequest = (identifyingKey: string): number => {
        return failedPullTenantRequests.findIndex((item) => item.Key === identifyingKey);
    };

    const getFailureMessageforRequest = (identifyingKey: string): string => {
        if (postActionErrorMessage) {
            if (postActionErrorMessage.includes(identifyingKey)) {
                return postActionErrorMessage;
            } else {
                return 'Failure';
            }
        }
        return 'Failure';
    };

    const onRenderDetailsHeader: IDetailsListProps['onRenderDetailsHeader'] = (props: any) => {
        return (
            <DetailsHeader
                {...props}
                onRenderColumnHeaderTooltip={(tooltipHostProps: any) => <TooltipHost {...tooltipHostProps} />}
            />
        );
    };

    const onRenderDetailsRow: IDetailsListProps['onRenderRow'] = (props: any) => {
        const customStyles: Partial<IDetailsRowStyles> = {};
        if (props && props.item) {
            const isOpen = getIdentifyingKey(props.item) === displayDocumentNumber;
            if (isOpen) {
                customStyles.root = { border: `1px solid` };
            }
            const rowNumber = (props.itemIndex ?? 0) + 2;
            return (
                <DetailsRow
                    {...props}
                    styles={customStyles}
                    aria-rowindex={rowNumber}
                    aria-label={`row ${rowNumber}`}
                />
            );
        }
        return null;
    };

    const onItemInvoked = (item?: any): void => {
        if (!isBulkSelected || !item) {
            return;
        }

        const itemKey = getIdentifyingKey(item);
        const isCurrentlySelected = selectionState.isKeySelected(itemKey);
        selectionState.setKeySelected(itemKey, !isCurrentlySelected, false);
    };

    const convertLaborHours: any = (timeString: string, laborHoursMeasure: string) => {
        if (!laborHoursMeasure) {
            laborHoursMeasure = 'Hrs';
        }
        return timeString + ' ' + laborHoursMeasure;
    };

    const getColSizeforDimension = (colSize: number): number => {
        if (colSize === ColSize.XXL || colSize === ColSize.XXXL) {
            return dimensions.width >= breakpointMap.xxxl
                ? ColSize.XXXL
                : dimensions.width >= 1440
                ? ColSize.Large
                : ColSize.Medium;
        } else if (colSize === ColSize.Large) {
            return dimensions.width >= breakpointMap.xxxl
                ? ColSize.XL
                : dimensions.width >= 1440
                ? ColSize.Large
                : ColSize.Medium;
        } else if (colSize === ColSize.Medium) {
            return dimensions.width >= breakpointMap.xxxl
                ? ColSize.XL
                : dimensions.width >= 1440
                ? ColSize.Medium
                : ColSize.Small;
        } else {
            return colSize;
        }
    };

    /* allows screen readers to provide infromation about context menu that opens on click of column header */
    const setAriaLabel = (columnName: string): string => {
        const labelExtension = isKeyboardColumnResizingOn ? COLUMN_HEADER_ACCESSIBILITY_LABEL : '';
        return columnName + labelExtension;
    };

    const allColumns: IColumn[] = [
        {
            key: 'IsRead',
            name: 'IsRead',
            ariaLabel: setAriaLabel('Status'),
            iconName: 'Page',
            className: fileIconCell,
            isIconOnly: true,
            isResizable: false,
            isSorted: sortedColumn === 'isRead',
            isSortedDescending: sortedColumn === 'isRead' ? isSortedDescending : true,
            fieldName: SummaryTableFieldNames.IsRead,
            minWidth: 45,
            maxWidth: 60,
            onRender: (item: any, index?: number) => {
                let icon;
                let statusLabel = 'unread request icon';
                const rowNumber = (index ?? 0) + 2;
                if (item.LastFailed) {
                    statusLabel = 'error icon';
                    icon = (
                        <Icon
                            title="Error Icon"
                            iconName="ReportWarning"
                            style={failedIconStyle}
                            ariaLabel={`row ${rowNumber} ${statusLabel}`}
                        />
                    );
                } else if (item['IsRead']) {
                    statusLabel = 'read request icon';
                    icon = <Styled.MailReadIcon title="Read Request Icon" />;
                } else {
                    icon = <Styled.MailUnreadIcon title="Unread Request Icon" />;
                }
                return (
                    <span role="img" aria-label={`row ${rowNumber} ${statusLabel}`}>
                        {icon}
                    </span>
                );
            },
        },
        {
            key: 'ApprovalIdentifier',
            name: 'Document Number',
            fieldName: SummaryTableFieldNames.ApprovalIdentifier,
            isResizable: true,
            isSorted: sortedColumn === 'ApprovalIdentifier',
            isSortedDescending: sortedColumn === 'ApprovalIdentifier' ? isSortedDescending : true,
            maxWidth: 200,
            invertAlignment: true,
            ariaLabel: setAriaLabel('Document Number'),
            onRender: (item: any) => {
                const isOpen = getIdentifyingKey(item) === displayDocumentNumber;
                return (
                    <TooltipHost content={item['ApprovalIdentifier']['DisplayDocumentNumber']}>
                        <Stack horizontal>
                            <Stack.Item align="center">
                                {isOpen || disabled ? (
                                    item['ApprovalIdentifier']['DisplayDocumentNumber']
                                ) : (
                                    <Link
                                        onKeyPress={imitateClickOnKeyPressForAnchor(() =>
                                            handleDocNumClicked(item['TenantId'], item['ApprovalIdentifier'])
                                        )}
                                        onClick={() =>
                                            handleDocNumClicked(item['TenantId'], item['ApprovalIdentifier'])
                                        }
                                        role="button"
                                        aria-label={
                                            focusedElement.nodeName.toString() === 'DIV'
                                                ? 'Document Number ' +
                                                  item['ApprovalIdentifier']['DisplayDocumentNumber'].toString()
                                                : 'Open Details'
                                        }
                                    >
                                        {item['ApprovalIdentifier']['DisplayDocumentNumber']}
                                    </Link>
                                )}
                            </Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
        {
            key: 'SubmittedDate',
            name: 'Submitted Date',
            fieldName: SummaryTableFieldNames.SubmittedDate,
            isResizable: true,
            isSorted: sortedColumn === 'SubmittedDate',
            isSortedDescending: sortedColumn === 'SubmittedDate' ? isSortedDescending : true,
            minWidth: ColSize.Medium,
            maxWidth: ColSize.Medium,
            ariaLabel: setAriaLabel('Submitted Date'),
            onRender: (item: any) => {
                return (
                    <Stack horizontal>
                        <Stack.Item align="center">
                            {new Date(item.SubmittedDate).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION)}
                        </Stack.Item>
                    </Stack>
                );
            },
        },
        {
            key: 'Submitter',
            name: 'Submitter',
            //type: 'custom',
            fieldName: SummaryTableFieldNames.Submitter,
            isResizable: true,
            isSorted: sortedColumn === 'Submitter',
            isSortedDescending: sortedColumn === 'Submitter' ? isSortedDescending : true,
            minWidth: ColSize.Large,
            maxWidth: ColSize.XL,
            ariaLabel: setAriaLabel('Submitter'),
            onRender: (item: any) => {
                if (item['Submitter']) {
                    return (
                        <TooltipHost content={item['Submitter']['Name']}>
                            <div
                                style={{
                                    display: 'flex',
                                    justifyContent: 'left',
                                    alignItems: 'center',
                                    flexWrap: 'nowrap',
                                }}
                                className="tooltip-on-hover"
                            >
                                <SubmitterPersona
                                    emailAlias={item['Submitter']['Alias']}
                                    size={PersonaSize.size24}
                                    imageAlt="Submitter Image"
                                />
                                <span
                                    style={{
                                        fontSize: '12px',
                                        color: TextColors.lightPrimary,
                                        overflow: 'hidden',
                                        textOverflow: 'ellipsis',
                                    }}
                                >
                                    {item['Submitter']['Name']}
                                </span>
                            </div>
                        </TooltipHost>
                    );
                }
            },
        },
        {
            key: 'Title',
            name: 'Title',
            //type: 'custom',
            fieldName: SummaryTableFieldNames.Title,
            isResizable: true,
            isSorted: sortedColumn === 'Title',
            isSortedDescending: sortedColumn === 'Title' ? isSortedDescending : true,
            minWidth: 120,
            maxWidth: 200,
            ariaLabel: setAriaLabel('Title'),
            onRender: (item: any) => {
                if (item['Title']) {
                    return (
                        <Stack horizontal>
                            <Stack.Item align="center">{item['Title']}</Stack.Item>
                        </Stack>
                    );
                }
            },
        },
        {
            key: 'UnitValue',
            name: 'Unit Value',
            //type: 'number',
            fieldName: SummaryTableFieldNames.UnitValue,
            isResizable: true,
            isSorted: sortedColumn === 'UnitValue',
            isSortedDescending: sortedColumn === 'UnitValue' ? isSortedDescending : true,
            minWidth: ColSize.Large,
            maxWidth: ColSize.Large,
            ariaLabel: setAriaLabel('Unit Value'),
            onRender: (item: any) => {
                const unitOfMeasure = item['UnitOfMeasure'] && item['UnitOfMeasure'] !== '-' ? item['UnitOfMeasure'] : '';
                return (
                    <Stack horizontal>
                       <Stack.Item>{`${formatUnitValue(item['UnitValue'])}${unitOfMeasure ? ' ' + unitOfMeasure : ''}`}</Stack.Item>
                    </Stack>
                );
            },
        },
        {
            key: 'CompanyCode',
            name: 'Company Code',
            fieldName: SummaryTableFieldNames.CompanyCode,
            isResizable: true,
            isSorted: sortedColumn === 'CompanyCode',
            isSortedDescending: sortedColumn === 'CompanyCode' ? isSortedDescending : true,
            maxWidth: 200,
            ariaLabel: setAriaLabel('Company Code'),
            onRender: (item: any) => {
                return (
                    <Stack horizontal>
                        <Stack.Item>{item['CompanyCode'] ? `${item['CompanyCode']}` : ''}</Stack.Item>
                    </Stack>
                );
            },
        },
        {
            key: 'CustomAttribute',
            name: 'Additional Information',
            //type: 'custom',
            fieldName: SummaryTableFieldNames.CustomAttribute,
            isResizable: true,
            isSorted: sortedColumn === 'CustomAttribute',
            isSortedDescending: sortedColumn === 'CustomAttribute' ? isSortedDescending : true,
            minWidth: 120,
            maxWidth: 300,
            ariaLabel: setAriaLabel('Additional Information'),
            onRender: (item: any) => {
                if (item['CustomAttribute']) {
                    return (
                        <Stack horizontal>
                            <Stack.Item align="center" grow>
                                {item['CustomAttribute']['CustomAttributeValue'] ? (
                                    item['CustomAttribute']['CustomAttributeName'] ? (
                                        typeof item['CustomAttribute']['CustomAttributeName'] === 'string' &&
                                        item['CustomAttribute']['CustomAttributeName'].endsWith(':') ? (
                                            <>
                                                {item['CustomAttribute']['CustomAttributeName']}{' '}
                                                {item['CustomAttribute']['CustomAttributeValue']}
                                            </>
                                        ) : (
                                            <>
                                                {item['CustomAttribute']['CustomAttributeName']}:{' '}
                                                {item['CustomAttribute']['CustomAttributeValue']}
                                            </>
                                        )
                                    ) : (
                                        item['CustomAttribute']['CustomAttributeValue']
                                    )
                                ) : (
                                    ''
                                )}
                            </Stack.Item>
                        </Stack>
                    );
                }
            },
        },
        // for Labor Management
        {
            key: 'allowInBulkApproval',
            name: 'Anomaly',
            fieldName: SummaryTableFieldNames.allowInBulkApproval,
            isSorted: sortedColumn === 'allowInBulkApproval',
            isSortedDescending: sortedColumn === 'allowInBulkApproval' ? isSortedDescending : true,
            minWidth: ColSize.Small,
            maxWidth: ColSize.Medium,
            isResizable: true,
            onRender: (item: any) => {
                const hasAnomaly = item?.actionDetails || false;
                let icon;
                let title = '';
                let exceptionReason = 'Standard';
                if (hasAnomaly) {
                    if (item.isLateApproval) {
                        title = 'Late Approval';
                    } else {
                        title = item?.actionDetails?.[0]?.actionType ?? '';
                    }
                }
                if (hasAnomaly) {
                    icon = <Icon title={title} iconName="Warning" style={failedIconStyle} />;
                    exceptionReason = item?.actionDetails?.[0]?.actionType ?? '';
                } else {
                    icon = <Icon title={title} iconName="Completed" style={pendingIconStyle} />;
                }
                return (
                    <TooltipHost content={exceptionReason}>
                        <Stack horizontal>
                            <Stack.Item styles={SharedStyled.StackStylesOverflowWithEllipsis}>
                                {icon}
                                {' ' + exceptionReason}
                            </Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
        {
            key: 'viewDetails',
            name: 'Details',
            fieldName: SummaryTableFieldNames.viewDetails,
            minWidth: ColSize.XS,
            maxWidth: ColSize.XS,
            isSorted: sortedColumn === 'viewDetails',
            isSortedDescending: sortedColumn === 'viewDetails' ? isSortedDescending : true,
            ariaLabel: setAriaLabel('Details'),
            onRender: (item: any) => {
                const isOpen = getIdentifyingKey(item) === displayDocumentNumber;
                return (
                    <Stack horizontal>
                        <Stack.Item>
                            {isOpen || disabled ? (
                                'View Details'
                            ) : (
                                <Link href="#" onClick={() => handleLaborClicked(item)}>
                                    View Details
                                </Link>
                            )}
                        </Stack.Item>
                    </Stack>
                );
            },
        },
        {
            key: 'actionDetails',
            name: 'Approval Status',
            fieldName: SummaryTableFieldNames.actionDetails,
            minWidth: ColSize.XS,
            maxWidth: ColSize.Small,
            isSorted: sortedColumn === 'actionDetails',
            isSortedDescending: sortedColumn === 'actionDetails' ? isSortedDescending : true,
            ariaLabel: setAriaLabel('Approval Status'),
            isResizable: true,
            onRender: (item: any) => {
                const identifyingKey = getIdentifyingKey(item);
                const statusMessage = 'Pending Action';
                let tooltipMessage = statusMessage;
                const bulkFailureIndex = isFailedBulkRequest(identifyingKey);
                const isBulkFailure = typeof bulkFailureIndex === 'number' && bulkFailureIndex >= 0;
                const isFailed = isBulkFailure || isFailedRequest(identifyingKey);
                if (isFailed) {
                    if (isBulkFailure) {
                        const prefix = documentNumberPrefix ? documentNumberPrefix + ' ' : '';
                        tooltipMessage =
                            prefix + identifyingKey + ' - ' + failedPullTenantRequests[bulkFailureIndex].Value;
                    } else {
                        tooltipMessage = getFailureMessageforRequest(identifyingKey);
                    }
                }
                return (
                    <TooltipHost content={tooltipMessage}>
                        <Stack horizontal>
                            <Stack.Item styles={SharedStyled.StackStylesOverflowWithEllipsis}>
                                {isFailed ? <SharedStyled.ErrorText>Failure</SharedStyled.ErrorText> : statusMessage}
                            </Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
        {
            key: 'submittedForFullName',
            name: 'Submitted For',
            fieldName: SummaryTableFieldNames.submittedForFullName,
            minWidth: getColSizeforDimension(ColSize.Medium),
            maxWidth: dimensions.width >= breakpointMap.xxxl ? ColSize.XXL : ColSize.XL,
            ariaLabel: setAriaLabel('Submitted For'),
            isSorted: sortedColumn === 'submittedForFullName',
            isSortedDescending: sortedColumn === 'submittedForFullName' ? isSortedDescending : true,
            isResizable: true,
            onRender: (item: any) => {
                const displayText = item['submittedForFullName'] ?? '';
                return (
                    <TooltipHost content={displayText}>
                        <Stack horizontal>
                            <Stack.Item
                                styles={SharedStyled.StackStylesOverflowWithEllipsis}
                            >{`${displayText}`}</Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
        {
            key: 'assignmentName',
            name: 'Assignment Name',
            ariaLabel: setAriaLabel('Assignment Name'),
            fieldName: SummaryTableFieldNames.assignmentName,
            minWidth: getColSizeforDimension(ColSize.Large),
            maxWidth: ColSize.XXXL,
            isSorted: sortedColumn === 'assignmentName',
            isSortedDescending: sortedColumn === 'assignmentName' ? isSortedDescending : true,
            isResizable: true,
            onRender: (item: any) => {
                const displayText = item?.assignmentDetails?.assignmentName || '';
                return (
                    <TooltipHost content={displayText}>
                        <Stack horizontal>
                            <Stack.Item styles={SharedStyled.StackStylesOverflowWithEllipsis}>
                                {`${displayText}`}
                            </Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
        {
            key: 'laborDate',
            name: 'Labor Date',
            ariaLabel: setAriaLabel('Labor Date'),
            fieldName: SummaryTableFieldNames.laborDate,
            minWidth: ColSize.XS,
            maxWidth: ColSize.Small,
            isSorted: sortedColumn === 'laborDate',
            isSortedDescending: sortedColumn === 'laborDate' ? isSortedDescending : true,
            isResizable: true,
            onRender: (item: any) => {
                return (
                    <Stack horizontal>
                        <Stack.Item>
                            {new Date(item.laborDate).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION)}
                        </Stack.Item>
                    </Stack>
                );
            },
        },
        {
            key: 'displayLaborHours',
            name: 'Labor Duration',
            fieldName: SummaryTableFieldNames.laborHours,
            minWidth: ColSize.XS,
            maxWidth: ColSize.Small,
            ariaLabel: setAriaLabel('Labor Duration'),
            isSorted: sortedColumn === 'displayLaborHours',
            isSortedDescending: sortedColumn === 'displayLaborHours' ? isSortedDescending : true,
            isResizable: true,
            onRender: (item: any) => {
                return (
                    <Stack horizontal>
                        <Stack.Item>
                            {convertLaborHours(item['displayLaborHours'], item['laborHoursMeasure'])}
                        </Stack.Item>
                    </Stack>
                );
            },
        },
        {
            key: 'laborCategoryName',
            name: 'Labor Category',
            fieldName: SummaryTableFieldNames.laborCategoryName,
            minWidth: ColSize.Small,
            ariaLabel: setAriaLabel('Labor Category'),
            isSorted: sortedColumn === 'laborCategoryName',
            isSortedDescending: sortedColumn === 'laborCategoryName' ? isSortedDescending : true,
            maxWidth: ColSize.Large,
            isResizable: true,
            onRender: (item: any) => {
                return (
                    <TooltipHost content={item['laborCategoryName']}>
                        <Stack horizontal>
                            <Stack.Item styles={SharedStyled.StackStylesOverflowWithEllipsis}>
                                {item['laborCategoryName']}
                            </Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
        {
            key: 'submittedByFullName',
            name: 'Submitted By',
            fieldName: SummaryTableFieldNames.submittedByFullName,
            minWidth: ColSize.Small,
            maxWidth: ColSize.Large,
            ariaLabel: setAriaLabel('Submitted By'),
            isSorted: sortedColumn === 'submittedByFullName',
            isSortedDescending: sortedColumn === 'submittedByFullName' ? isSortedDescending : true,
            isResizable: true,
            onRender: (item: any) => {
                const displayText = item['submittedByFullName'] ?? '';
                return (
                    <TooltipHost content={displayText}>
                        <Stack horizontal>
                            <Stack.Item
                                styles={SharedStyled.StackStylesOverflowWithEllipsis}
                            >{`${displayText}`}</Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
        {
            key: 'isBillable',
            name: 'Is Billable',
            fieldName: SummaryTableFieldNames.isBillable,
            minWidth: ColSize.XXS,
            ariaLabel: setAriaLabel('Is Billable'),
            maxWidth: ColSize.XXS,
            isSorted: sortedColumn === 'isBillable',
            isSortedDescending: sortedColumn === 'isBillable' ? isSortedDescending : true,
            isResizable: true,
            onRender: (item: any) => {
                return (
                    <Stack horizontal>
                        <Stack.Item>{`${
                            booleanToReadableValue(item?.assignmentDetails?.isBillable) || ''
                        }`}</Stack.Item>
                    </Stack>
                );
            },
        },
        {
            key: 'laborNotes',
            name: 'Notes',
            fieldName: SummaryTableFieldNames.laborNotes,
            minWidth: ColSize.Small,
            isResizable: true,
            isSorted: sortedColumn === 'laborNotes',
            isSortedDescending: sortedColumn === 'laborNotes' ? isSortedDescending : true,
            ariaLabel: setAriaLabel('Notes'),
            onRender: (item: any) => {
                const notesValue = item?.['laborNotes'] || '';
                return (
                    <TooltipHost content={notesValue}>
                        <Stack horizontal>
                            <Stack.Item
                                styles={SharedStyled.StackStylesOverflowWithEllipsis}
                            >{`${notesValue}`}</Stack.Item>
                        </Stack>
                    </TooltipHost>
                );
            },
        },
    ];

    /* TODO: custom pagination
    const paginationProps: ICoherencePaginationProps = {
        pageCount: pageCount,
        selectedPage: selectedPage,
        previousPageAriaLabel: `previous page - ${props.tenantName}`,
        nextPageAriaLabel: `next page - ${props.tenantName}`,
        inputFieldAriaLabel: `page number - ${props.tenantName}`,
        telemetryHook: telemetryClient,
        onPageChange: (pageNumber: number) => {
            if (pageNumber !== selectedPage) {
                setSelectedPage(pageNumber);
                setTableRecords(
                    props.tenantGroup.slice(rowsPerPage * pageNumber - rowsPerPage, rowsPerPage * pageNumber)
                );
            }
        },
    };
    */

    const tenantToColumnMapping: Dictionary<string[]> = {
        Default: [
            SummaryTableFieldNames.IsRead,
            SummaryTableFieldNames.ApprovalIdentifier,
            SummaryTableFieldNames.SubmittedDate,
            SummaryTableFieldNames.Submitter,
            SummaryTableFieldNames.Title,
            SummaryTableFieldNames.UnitValue,
            SummaryTableFieldNames.CompanyCode,
            SummaryTableFieldNames.CustomAttribute,
        ],
        PullTenant: [
            SummaryTableFieldNames.viewDetails,
            SummaryTableFieldNames.actionDetails,
            SummaryTableFieldNames.submittedForFullName,
            SummaryTableFieldNames.assignmentName,
            SummaryTableFieldNames.laborDate,
            SummaryTableFieldNames.laborHours,
            SummaryTableFieldNames.laborCategoryName,
            SummaryTableFieldNames.submittedByFullName,
            SummaryTableFieldNames.isBillable,
            SummaryTableFieldNames.laborNotes,
            SummaryTableFieldNames.allowInBulkApproval,
        ],
        Mobile: [SummaryTableFieldNames.ApprovalIdentifier, SummaryTableFieldNames.Submitter],
    };

    // will accept tenant type: all | pullTenant — returns columns in the user's saved order
    function renderColumns(appName: string) {
        const ordered = appName === 'pullTenant' ? visibleColumnsPullTenant : visibleColumnsDefault;
        return ordered
            .map((fieldName: string) => allColumns.find((col) => col.fieldName === fieldName))
            .filter(Boolean) as IColumn[];
    }

    function handleColumnReorder(draggedIndex: number, targetIndex: number) {
        const currentCols = isPullTenantSelected ? visibleColumnsPullTenant : visibleColumnsDefault;
        const reordered = [...currentCols];
        const [removed] = reordered.splice(draggedIndex, 1);
        reordered.splice(targetIndex, 0, removed);
        dispatch(updateVisibleColumns(isPullTenantSelected ? 'pullTenant' : 'all', reordered));
    }

    function onSort(event: React.MouseEvent<HTMLElement, MouseEvent>, column: IColumn): void {
        event.preventDefault();

        // Determine new sort direction
        const newDescending = sortedColumn === column.key ? !isSortedDescending : false;

        // Update state
        setIsSortedDescending(newDescending);
        setSortedColumn(column.key);

        // Sort items using shared helper
        const sortedRecords = sortItems(summaryTableRecords, column.key, newDescending);

        // Update selection state and table records
        selectionState.setItems(sortedRecords, true);
        setTableRecords(sortedRecords);
    }

    const dialogStyles = { main: { maxWidth: 450 } };
    const resizeDialogContentProps = {
        type: DialogType.normal,
        title: 'Resize Column',
        closeButtonAriaLabel: 'Close',
        subText: 'Enter desired column width pixels:',
    };

    const modalProps = {
        titleAriaId: 'Dialog',
        subtitleAriaId: 'Dialog sub',
        isBlocking: false,
        styles: dialogStyles,
    };

    const hideDialog = () => setIsDialogHidden(true);

    const showDialog = () => setIsDialogHidden(false);

    const resizeColumn = (column: IColumn) => {
        columnToEdit.current = column;
        clickHandler.current = RESIZE;
        showDialog();
    };

    const onHideContextualMenu = React.useCallback(() => setContextualMenuProps(undefined), []);

    const onColumnClickAccessible = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        setContextualMenuProps(getContextualMenuProps(ev, column));
    };

    const getContextualMenuProps = (ev: React.MouseEvent<HTMLElement>, column: IColumn): IContextualMenuProps => {
        const items = [
            { key: 'resize', text: 'Resize', onClick: () => resizeColumn(column) },
            { key: 'sort', text: 'Sort', onClick: () => onSort(ev, column) },
        ];

        return {
            items: items,
            target: ev.currentTarget as HTMLElement,
            gapSpace: 10,
            isBeakVisible: true,
            onDismiss: onHideContextualMenu,
        };
    };

    const confirmDialog = () => {
        const detailsList = detailsListRef.current;

        if (textfieldRef.current) {
            input.current = Number(textfieldRef.current.value);
        }

        if (columnToEdit.current && input.current && detailsList) {
            if (clickHandler.current === RESIZE) {
                const width = input.current;
                detailsList.updateColumn(columnToEdit.current, { width: width });
            }
        }

        input.current = null;
        hideDialog();
    };

    return (
        <Styled.TableGlobalStyles>
            <div
                style={
                    isSingleGroupShown
                        ? {
                              position: 'relative',
                              height: `${
                                  dimensions.width <= 480
                                      ? dimensions.height * 0.65
                                      : dimensions.width < 1024
                                      ? dimensions.height
                                      : isBulkSelected
                                      ? dimensions.height - SharedStyled.bulkTableViewBottomOffset
                                      : dimensions.height - 300
                              }px`,
                              overflowY: 'scroll',
                          }
                        : isPaginationEnabled
                        ? {}
                        : { height: `${Math.min((props.tenantGroup.length + 1) * 50, 250)}px`, overflowY: 'scroll' }
                }
            >
                <DetailsList
                    componentRef={detailsListRef}
                    ariaLabel={`${props.tenantName} table`}
                    columns={renderColumns(isPullTenantSelected ? 'pullTenant' : 'all')}
                    items={summaryTableRecords}
                    selectionMode={isBulkSelected ? SelectionMode.multiple : SelectionMode.none}
                    checkboxVisibility={CheckboxVisibility.always}
                    selection={selectionState}
                    ariaLabelForSelectionColumn={`Toggle selection column - ${props.tenantName}`}
                    ariaLabelForSelectAllCheckbox={`Toggle selection for all items - ${props.tenantName}`}
                    checkButtonAriaLabel={`Row checkbox - ${props.tenantName}`}
                    selectionPreservedOnEmptyClick={true}
                    selectionZoneProps={{
                        selection: selectionState,
                        disableAutoSelectOnInputElements: true,
                        isSelectedOnFocus: false,
                        toggleWithoutModifierPressed: true,
                    }}
                    onItemInvoked={onItemInvoked}
                    setKey="multiple"
                    onRenderDetailsHeader={onRenderDetailsHeader as any}
                    onRenderRow={onRenderDetailsRow as any}
                    onColumnHeaderClick={isKeyboardColumnResizingOn ? onColumnClickAccessible : onSort}
                    columnReorderOptions={{
                        frozenColumnCountFromStart: isBulkSelected ? 1 : 0,
                        handleColumnReorder: handleColumnReorder,
                    }}
                    layoutMode={
                        isSingleGroupShown ? DetailsListLayoutMode.fixedColumns : DetailsListLayoutMode.justified
                    }
                />
            </div>
            {contextualMenuProps && <ContextualMenu {...contextualMenuProps} />}
            <Dialog
                hidden={isDialogHidden}
                onDismiss={hideDialog}
                dialogContentProps={resizeDialogContentProps}
                modalProps={modalProps}
            >
                <TextField componentRef={textfieldRef} ariaLabel={'Enter column width'} />
                <DialogFooter>
                    <PrimaryButton onClick={confirmDialog} text={clickHandler.current} />
                    <DefaultButton onClick={hideDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
            {/* {isPaginationEnabled && (
                <Stack horizontal horizontalAlign="end">
                    <Stack horizontal horizontalAlign="space-between" styles={paginationWidth}>
                        <div style={paginationAlign}>
                            <CoherencePagination {...paginationProps} />
                        </div>
                    </Stack>
                </Stack>
            )} */}
        </Styled.TableGlobalStyles>
    );
}

export default SummaryTableColumns;
