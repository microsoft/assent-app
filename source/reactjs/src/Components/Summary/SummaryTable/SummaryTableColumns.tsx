/* eslint-disable react/display-name */
import * as React from 'react';
import * as Styled from './SummaryTableStyling';
import * as SharedStyled from '../../Shared/SharedLayout';
import { failedIconStyle, fileIconCell, paginationWidth, paginationAlign } from './SummaryTableStyling';
import { ISummaryRecordModel } from './SummaryTable.types';
import { SummaryTableFieldNames } from './SummaryTableFieldNames';
import { booleanToReadableValue, imitateClickOnKeyPressForAnchor } from '../../../Helpers/sharedHelpers';
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
    getIsPullTenantSelected,
    getSelectedApprovalRecords,
    getTableRowCount,
} from '../../Shared/SharedComponents.selectors';
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
    const isPullTenantSelected = useSelector(getIsPullTenantSelected);
    const failedPullTenantRequests = useSelector(getFailedPullTenantRequests);
    const filteredTenantInfo = useSelector(getFilteredTenantInfo);
    const tableRowCount = useSelector(getTableRowCount);
    const disabled = useSelector(getIsDisabled);
    const isKeyboardColumnResizingOn = useSelector(getIsKeyboardColumnResizingOn);

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
        if (isPullTenantSelected) {
            return item['allowInBulkApproval'] && (item?.isSelectable ?? true);
        } else {
            const res = item['IsControlsAndComplianceRequired']
                ? item['IsRead'] && (item?.isSelectable ?? true)
                : item?.isSelectable ?? true;
            return res;
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
            selectionState.setItems(newItems);
            setTableRecords(newItems);
            setAreItemsUpdated(true);
        } else if (!isAllChecked && areItemsUpdated && !isSelectAllConfigured) {
            selectionState.setAllSelected(true);
            setIsSelectAllConfigured(true);
        } else if (!isAllChecked && areItemsUpdated && isSelectAllConfigured) {
            const clearedItems = clearMaxSelection(selectionState.getItems());
            setSavedSelection(selectionState.getSelection());
            selectionState.setItems(clearedItems);
            setTableRecords(clearedItems);
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
            setTableRecords(records);
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
        setTableRecords(temp);
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
            setTableRecords(withSelection);
        } else {
            setTableRecords(props.tenantGroup);
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
            dispatch(updateMyRequest(Number(tenantId), docNum, displayDocNum, fiscalYear));
            dispatch(updatePanelState(true));
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
            return <DetailsRow {...props} styles={customStyles} />;
        }
        return null;
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
            onRender: (item: any) => {
                let lastFailedLocal = item.LastFailed;
                let icon;
                if (lastFailedLocal) {
                    icon = <Icon title="Error Icon" iconName="ReportWarning" style={failedIconStyle} />;
                } else if (item['IsRead']) {
                    icon = <Styled.MailReadIcon title="Read Request Icon" />;
                } else if (!item['IsRead']) {
                    icon = <Styled.MailUnreadIcon title="Unread Request Icon" />;
                } else {
                    null;
                }
                return icon;
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
                return (
                    <Stack horizontal>
                        <Stack.Item>{`${item['UnitValue']} ${item['UnitOfMeasure']}`}</Stack.Item>
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
                                {item['CustomAttribute']['CustomAttributeValue']}
                            </Stack.Item>
                        </Stack>
                    );
                }
            },
        },
        // for Labor Management
        {
            key: 'allowInBulkApproval',
            name: 'allowInBulkApproval',
            ariaLabel: setAriaLabel('Status'),
            iconName: 'Info',
            isIconOnly: true,
            fieldName: SummaryTableFieldNames.allowInBulkApproval,
            isSorted: sortedColumn === 'allowInBulkApproval',
            isSortedDescending: sortedColumn === 'allowInBulkApproval' ? isSortedDescending : true,
            minWidth: 25,
            maxWidth: 25,
            onRender: (item: any) => {
                let allowed = item['allowInBulkApproval'] ?? true;
                let icon;
                let title = '';
                if (!allowed) {
                    if (item.isLateApproval) {
                        title = 'Late Approval';
                    } else {
                        title = item?.actionDetails?.[0]?.actionType ?? '';
                    }
                }
                if (!allowed) {
                    icon = <Icon title={title} iconName="Warning" style={failedIconStyle} />;
                } else {
                    null;
                }
                return icon;
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
            isSorted: sortedColumn === 'laborHours',
            isSortedDescending: sortedColumn === 'laborHours' ? isSortedDescending : true,
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

    // will accept tenant type: all | pullTenant
    function renderColumns(appName: string) {
        // if (windowWidth < 640) {
        //     return allColumns.filter(column => tenantToColumnMapping['Mobile'].includes(column.fieldName));
        // }
        switch (appName) {
            // allColumns is created by filtering with tenant mapped columns
            case 'all':
                return allColumns.filter((column) => tenantToColumnMapping['Default'].includes(column.fieldName));
            case 'pullTenant':
                return allColumns.filter((column) => tenantToColumnMapping['PullTenant'].includes(column.fieldName));
            default:
                return allColumns.filter((column) => tenantToColumnMapping['Default'].includes(column.fieldName));
        }
    }

    function onSort(event: React.MouseEvent<HTMLElement, MouseEvent>, column: IColumn): void {
        event.preventDefault();
        //if no column is currently sorted, keep default false value for isSortedDescending
        if (sortedColumn !== column.key) {
            setIsSortedDescending(false);
        } else {
            setIsSortedDescending(!isSortedDescending);
        }
        setSortedColumn(column.key);
        switch (column.key) {
            case 'ApprovalIdentifier':
                if (column.isSorted) {
                    if (column.isSortedDescending) {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['ApprovalIdentifier']['DisplayDocumentNumber'].localeCompare(
                                b['ApprovalIdentifier']['DisplayDocumentNumber'],
                                'en-US',
                                {
                                    numeric: true,
                                    sensitivity: 'base',
                                }
                            )
                        );
                    } else {
                        props.tenantGroup.sort((a: any, b: any) =>
                            b['ApprovalIdentifier']['DisplayDocumentNumber'].localeCompare(
                                a['ApprovalIdentifier']['DisplayDocumentNumber'],
                                'en-US',
                                {
                                    numeric: true,
                                    sensitivity: 'base',
                                }
                            )
                        );
                    }
                } else {
                    props.tenantGroup.sort((a: any, b: any) =>
                        a['ApprovalIdentifier']['DisplayDocumentNumber'].localeCompare(
                            b['ApprovalIdentifier']['DisplayDocumentNumber'],
                            'en-US',
                            {
                                numeric: true,
                                sensitivity: 'base',
                            }
                        )
                    );
                }

                if (isPaginationEnabled) {
                    setTableRecords(
                        props.tenantGroup.slice(rowsPerPage * selectedPage - rowsPerPage, rowsPerPage * selectedPage)
                    );
                } else {
                    setTableRecords(props.tenantGroup);
                }
                break;
            case 'Submitter':
                if (column.isSorted) {
                    if (column.isSortedDescending) {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['Submitter']['Name'] > b['Submitter']['Name'] ? 1 : -1
                        );
                    } else {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['Submitter']['Name'] < b['Submitter']['Name'] ? 1 : -1
                        );
                    }
                } else {
                    props.tenantGroup.sort((a: any, b: any) =>
                        a['Submitter']['Name'] > b['Submitter']['Name'] ? 1 : -1
                    );
                }
                if (isPaginationEnabled) {
                    setTableRecords(
                        props.tenantGroup.slice(rowsPerPage * selectedPage - rowsPerPage, rowsPerPage * selectedPage)
                    );
                } else {
                    setTableRecords(props.tenantGroup);
                }
                break;
            case 'UnitValue':
                if (column.isSorted) {
                    if (column.isSortedDescending) {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['UnitValue'].localeCompare(b['UnitValue'], 'en-US', {
                                numeric: true,
                                sensitivity: 'base',
                            })
                        );
                    } else {
                        props.tenantGroup.sort((a: any, b: any) =>
                            b['UnitValue'].localeCompare(a['UnitValue'], 'en-US', {
                                numeric: true,
                                sensitivity: 'base',
                            })
                        );
                    }
                } else {
                    props.tenantGroup.sort((a: any, b: any) =>
                        a['UnitValue'].localeCompare(b['UnitValue'], 'en-US', {
                            numeric: true,
                            sensitivity: 'base',
                        })
                    );
                }

                if (isPaginationEnabled) {
                    setTableRecords(
                        props.tenantGroup.slice(rowsPerPage * selectedPage - rowsPerPage, rowsPerPage * selectedPage)
                    );
                } else {
                    setTableRecords(props.tenantGroup);
                }
                break;
            case 'actionDetails':
                if (column.isSorted) {
                    if (column.isSortedDescending) {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['actionDetails']['actionType'] > b['actionDetails']['actionType'] ? 1 : -1
                        );
                    } else {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['actionDetails']['actionType'] < b['actionDetails']['actionType'] ? 1 : -1
                        );
                    }
                } else {
                    props.tenantGroup.sort((a: any, b: any) =>
                        a['actionDetails']['actionType'] > b['actionDetails']['actionType'] ? 1 : -1
                    );
                }
                if (isPaginationEnabled) {
                    setTableRecords(
                        props.tenantGroup.slice(rowsPerPage * selectedPage - rowsPerPage, rowsPerPage * selectedPage)
                    );
                } else {
                    setTableRecords(props.tenantGroup);
                }
                break;
            case 'assignmentName':
                if (column.isSorted) {
                    if (column.isSortedDescending) {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['assignmentDetails']['assignmentName'] > b['assignmentDetails']['assignmentName'] ? 1 : -1
                        );
                    } else {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['assignmentDetails']['assignmentName'] < b['assignmentDetails']['assignmentName'] ? 1 : -1
                        );
                    }
                } else {
                    props.tenantGroup.sort((a: any, b: any) =>
                        a['assignmentDetails']['assignmentName'] > b['assignmentDetails']['assignmentName'] ? 1 : -1
                    );
                }
                if (isPaginationEnabled) {
                    setTableRecords(
                        props.tenantGroup.slice(rowsPerPage * selectedPage - rowsPerPage, rowsPerPage * selectedPage)
                    );
                } else {
                    setTableRecords(props.tenantGroup);
                }
                break;
            case 'isBillable':
                if (column.isSorted) {
                    if (column.isSortedDescending) {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['assignmentDetails']['isBillable'] > b['assignmentDetails']['isBillable'] ? 1 : -1
                        );
                    } else {
                        props.tenantGroup.sort((a: any, b: any) =>
                            a['assignmentDetails']['isBillable'] < b['assignmentDetails']['isBillable'] ? 1 : -1
                        );
                    }
                } else {
                    props.tenantGroup.sort((a: any, b: any) =>
                        a['assignmentDetails']['isBillable'] > b['assignmentDetails']['isBillable'] ? 1 : -1
                    );
                }
                if (isPaginationEnabled) {
                    setTableRecords(
                        props.tenantGroup.slice(rowsPerPage * selectedPage - rowsPerPage, rowsPerPage * selectedPage)
                    );
                } else {
                    setTableRecords(props.tenantGroup);
                }
                break;
            default:
                const isSortedDescendingNew = column.isSorted ? !column.isSortedDescending : false;
                const key = column.key;
                props.tenantGroup.sort((a: T, b: T) =>
                    (isSortedDescendingNew ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1
                );
                setTableRecords(props.tenantGroup);
                break;
        }
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
        <React.Fragment>
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
                    selectionPreservedOnEmptyClick={false}
                    selectionZoneProps={{ selection: selectionState, disableAutoSelectOnInputElements: true }}
                    setKey="multiple"
                    onRenderDetailsHeader={onRenderDetailsHeader as any}
                    onRenderRow={onRenderDetailsRow as any}
                    onColumnHeaderClick={isKeyboardColumnResizingOn ? onColumnClickAccessible : onSort}
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
        </React.Fragment>
    );
}

export default SummaryTableColumns;
