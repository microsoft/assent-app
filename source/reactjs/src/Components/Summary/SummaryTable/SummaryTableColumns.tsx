/* eslint-disable react/display-name */
import * as React from 'react';
import * as Styled from './SummaryTableStyling';
import * as SharedStyled from '../../Shared/SharedLayout';
import { failedIconStyle, fileIconCell, paginationWidth, paginationAlign } from './SummaryTableStyling';
import { ISummaryRecordModel } from './SummaryTable.types';
import { SummaryTableFieldNames } from './SummaryTableFieldNames';
import { booleanToReadableValue, imitateClickOnKeyPressForAnchor } from '../../../Helpers/sharedHelpers';
import { Dictionary } from 'adaptivecards';
import { TooltipHost, IDetailsListProps, DetailsHeader } from '@fluentui/react';
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

function SummaryTableColumns(props: any): React.ReactElement {
    const { useSelector, dispatch, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );

    const isLoadingSummary = useSelector(getIsLoadingSummary);
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
    const [initialSortColumn] = React.useState(
        isPullTenantSelected ? tableColumns?.find((col) => col.isDefaultSortColumn)?.field : 'SubmittedDate'
    );

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

    const allColumns: IColumn[] = [
        {
            key: 'IsRead',
            type: 'custom',
            name: 'IsRead',
            ariaLabel: 'Status',
            iconName: 'Page',
            className: fileIconCell,
            isIconOnly: true,
            isResizable: false,
            fieldName: SummaryTableFieldNames.IsRead,
            minColumnWidth: 45,
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
            type: 'number',
            fieldName: SummaryTableFieldNames.ApprovalIdentifier,
            isResizable: true,
            maxWidth: 200,
            invertAlignment: true,
            ariaLabel: 'Document Number',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.SubmittedDate,
            isResizable: true,
            minColumnWidth: ColSize.Medium,
            maxWidth: ColSize.Medium,
            ariaLabel: 'Submitted Date',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.Submitter,
            isResizable: true,
            minColumnWidth: ColSize.Large,
            maxWidth: ColSize.XL,
            ariaLabel: 'Submitter',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.Title,
            isResizable: true,
            minColumnWidth: 120,
            maxWidth: 200,
            ariaLabel: 'Title',
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
            type: 'number',
            fieldName: SummaryTableFieldNames.UnitValue,
            isResizable: true,
            minColumnWidth: ColSize.Large,
            maxWidth: ColSize.Large,
            ariaLabel: 'Unit Value',
            invertAlignment: true,
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.CompanyCode,
            isResizable: true,
            maxWidth: 200,
            ariaLabel: 'Company Code',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.CustomAttribute,
            isResizable: true,
            minColumnWidth: 120,
            maxWidth: 300,
            ariaLabel: 'Additional Information',
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
            ariaLabel: 'Status',
            iconName: 'Info',
            isIconOnly: true,
            type: 'custom',
            fieldName: SummaryTableFieldNames.allowInBulkApproval,
            minColumnWidth: 25,
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.viewDetails,
            minColumnWidth: ColSize.XS,
            maxWidth: ColSize.XS,
            ariaLabel: 'Details',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.actionDetails,
            minColumnWidth: ColSize.XS,
            maxWidth: ColSize.Small,
            ariaLabel: 'Approval Status',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.submittedForFullName,
            minColumnWidth: getColSizeforDimension(ColSize.Medium),
            maxWidth: dimensions.width >= breakpointMap.xxxl ? ColSize.XXL : ColSize.XL,
            ariaLabel: 'Submitted For',
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
            type: 'custom',
            ariaLabel: 'Assignment Name',
            fieldName: SummaryTableFieldNames.assignmentName,
            minColumnWidth: getColSizeforDimension(ColSize.Large),
            maxWidth: ColSize.XXXL,
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
            type: 'custom',
            ariaLabel: 'Labor Date',
            fieldName: SummaryTableFieldNames.laborDate,
            minColumnWidth: ColSize.XS,
            maxWidth: ColSize.Small,
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
            key: 'laborHours',
            name: 'Labor Duration',
            type: 'custom',
            fieldName: SummaryTableFieldNames.laborHours,
            minColumnWidth: ColSize.XS,
            maxWidth: ColSize.Small,
            ariaLabel: 'Labor Duration',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.laborCategoryName,
            minColumnWidth: ColSize.Small,
            ariaLabel: 'Labor Category',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.submittedByFullName,
            minColumnWidth: ColSize.Small,
            maxWidth: ColSize.Large,
            ariaLabel: 'Submitted By',
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.isBillable,
            minColumnWidth: ColSize.XXS,
            ariaLabel: 'Is Billable',
            maxWidth: ColSize.XXS,
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
            type: 'custom',
            fieldName: SummaryTableFieldNames.laborNotes,
            minColumnWidth: ColSize.Small,
            isResizable: true,
            ariaLabel: 'Notes',
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
                break;
        }
    }

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
                    onColumnHeaderClick={onSort as any}
                    layoutMode={
                        isSingleGroupShown ? DetailsListLayoutMode.fixedColumns : DetailsListLayoutMode.justified
                    }
                />
            </div>
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
