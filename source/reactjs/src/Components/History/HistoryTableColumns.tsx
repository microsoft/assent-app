/* eslint-disable react/display-name */
import * as Styled from './HistoryStyling';
import {
    DetailsListLayoutMode,
    DetailsRow,
    IDetailsRowStyles,
    Selection,
    SelectionMode,
    DetailsList,
    IColumn,
} from '@fluentui/react/lib/DetailsList';
import { TextColors } from '../Shared/SharedColors';
import { IHistoryRecordModel } from './History.types';
import { HistoryRecordFieldNames } from './HistoryRecordFieldNames';
import { imitateClickOnKeyPressForAnchor, safeJSONParse } from '../../Helpers/sharedHelpers';
import * as React from 'react';
import { Dictionary } from 'adaptivecards';
import { mapDate } from '../Shared/Components/DateFormatting';

import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { sharedComponentsReducerName, sharedComponentsInitialState } from '../Shared/SharedComponents.reducer';
import { IComponentsAppState } from '../Shared/SharedComponents.types';
import { TooltipHost, IDetailsListProps, DetailsHeader } from '@fluentui/react';
import { requestMyHistory, updatePanelState } from '../Shared/SharedComponents.actions';
import { updateMyRequest } from '../Shared/Details/Details.actions';
import { withContext } from '@micro-frontend-react/employee-experience/lib/Context';
import { Stack } from '@fluentui/react/lib/Stack';
import { PersonaSize } from '../Shared/Components/Persona/Persona.types';
import { getTenantIcon } from '../Shared/Components/IconMapping';
import { useActiveElement } from '../Shared/Components/ActiveElement';
import { SubmitterPersona } from '../Shared/Components/SubmitterPersona';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { Link } from '../Shared/Styles/Link';

function HistoryColumns(): React.ReactElement {
    const { useSelector, dispatch, telemetryClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const firstUpdate = React.useRef(true);

    const {
        historySelectedPage,
        sortColumnField,
        sortDirection,
        historyTimePeriod,
        historySearchCriteria,
        isLoadingHistory,
        historyData,
        tenantInfo,
        historyGroupedBy,
        filterValue,
        historyTenantIdFilter,
        historyTotalRecords
    } = useSelector(
        (state: IComponentsAppState) => state.dynamic?.[sharedComponentsReducerName] || sharedComponentsInitialState
    );
    const [windowWidth, setWindowWidth] = React.useState<number>(0);
    const focusedElement = useActiveElement();

    function handleResize() {
        setWindowWidth(window.innerWidth);
    }

    React.useEffect(() => {
        handleResize();
        window.addEventListener('resize', handleResize);

        return (): void => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    function onSort(event: React.MouseEvent<HTMLElement, MouseEvent>, column: IColumn): void {
        event.preventDefault();
        if (column.key != sortColumnField) {
            dispatch(
                requestMyHistory(
                    historySelectedPage,
                    column.key,
                    'ASC',
                    historySearchCriteria,
                    historyTimePeriod,
                    historyTenantIdFilter
                )
            );
        } else {
            // if column selected is clicked one more time reverse the order direction
            switch (sortDirection) {
                case 'DESC':
                    dispatch(
                        requestMyHistory(
                            historySelectedPage,
                            sortColumnField,
                            'ASC',
                            historySearchCriteria,
                            historyTimePeriod,
                            historyTenantIdFilter
                        )
                    );
                    break;
                default:
                    dispatch(
                        requestMyHistory(
                            historySelectedPage,
                            sortColumnField,
                            'DESC',
                            historySearchCriteria,
                            historyTimePeriod,
                            historyTenantIdFilter
                        )
                    );
                    break;
            }
        }
    }

    const handleDocNumClicked = (tenantId: string, jsonString: string, historyItem: any): void => {
        // serialize the jsonData and get docNum and displayDocNum from there
        const jsonData = safeJSONParse(jsonString);
        let docNum = jsonData?.ApprovalIdentifier?.DocumentNumber ?? historyItem['DocumentNumber'];
        let displayDocNum = jsonData?.ApprovalIdentifier?.DisplayDocumentNumber ?? historyItem['DocumentNumber'];
        let fiscalYear = jsonData?.ApprovalIdentifier?.FiscalYear ?? '';
        dispatch(updateMyRequest(Number(tenantId), docNum, displayDocNum, fiscalYear, historyItem));
        dispatch(updatePanelState(true));
    };
    const onRenderDetailsHeader: IDetailsListProps['onRenderDetailsHeader'] = (props: any) => {
        return (
            <DetailsHeader
                {...props}
                onRenderColumnHeaderTooltip={(tooltipHostProps: any) => <TooltipHost {...tooltipHostProps} />}
            />
        );
    };
    const allColumns: IColumn[] = [
        {
            key: 'DocumentNumber',
            name: 'Request Number',
            type: 'custom',
            fieldName: HistoryRecordFieldNames.DocumentNumber,
            isResizable: true,
            maxWidth: 200,
            onRender: (item: any) => {
                return (
                    <Stack horizontal>
                        <Stack.Item align="center">
                            <Link
                                style={{ fontSize: '12px' }}
                                onKeyPress={imitateClickOnKeyPressForAnchor(() =>
                                    handleDocNumClicked(item['TenantId'], item['JsonData'], item)
                                )}
                                onClick={() => handleDocNumClicked(item['TenantId'], item['JsonData'], item)}
                                role="button"
                                aria-label={
                                    focusedElement.nodeName.toString() === 'DIV'
                                        ? 'Document Number ' + item['DocumentNumber'].toString()
                                        : 'Open Details'
                                }
                            >
                                {item['DocumentNumber']}
                            </Link>
                        </Stack.Item>
                    </Stack>
                );
            }
        },
        {
            key: 'ActionDate',
            name: 'Date',
            type: 'custom',
            fieldName: HistoryRecordFieldNames.ActionDate,
            isResizable: true,
            maxWidth: 140,
            onRender: (item: string[]) => {
                return (
                    <Stack horizontal>
                        <Stack.Item align="center">
                            <span style={{ fontSize: '12px', color: TextColors.lightPrimary }}>
                                {mapDate(item['ActionDate' as any])}
                            </span>
                        </Stack.Item>
                    </Stack>
                );
            }
        },
        {
            key: 'SubmitterName',
            name: 'Submitter',
            type: 'custom',
            fieldName: HistoryRecordFieldNames.SubmitterName,
            isResizable: true,
            minColumnWidth: 120,
            maxWidth: 200,
            onRender: (item: any) => {
                return (
                    <TooltipHost content={item['SubmitterName']}>
                        <div
                            style={{
                                display: 'flex',
                                justifyContent: 'left',
                                alignItems: 'center',
                                flexWrap: 'nowrap'
                            }}
                            className="tooltip-on-hover"
                        >
                            <SubmitterPersona
                                emailAlias={item['SubmittedAlias']}
                                size={PersonaSize.size24}
                                imageAlt="Submitter Image"
                            />
                            <span
                                style={{
                                    fontSize: '12px',
                                    color: TextColors.lightPrimary,
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis'
                                }}
                            >
                                {item['SubmitterName']}
                            </span>
                        </div>
                    </TooltipHost>
                );
            }
        },
        {
            key: 'AppName',
            name: 'Application',
            type: 'custom',
            fieldName: HistoryRecordFieldNames.Tenant,
            isResizable: true,
            minColumnWidth: 120,
            maxWidth: 200,
            onRender: (item: any) => {
                return (
                    <TooltipHost content={item['AppName']}>
                        <div
                            style={{
                                display: 'flex',
                                justifyContent: 'left',
                                alignItems: 'center',
                                flexWrap: 'nowrap'
                            }}
                        >
                            <Styled.HistoryColumnTenantImage>
                                {getTenantIcon(item['AppName'], tenantInfo, '22px')}
                            </Styled.HistoryColumnTenantImage>
                            <span
                                style={{
                                    fontSize: '12px',
                                    color: TextColors.lightPrimary,
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis'
                                }}
                            >
                                {item['AppName']}
                            </span>
                        </div>
                    </TooltipHost>
                );
            }
        },
        {
            key: 'Title',
            name: 'Title',
            type: 'custom',
            fieldName: HistoryRecordFieldNames.Title,
            isResizable: true,
            maxWidth: 200,
            onRender: (item: any) => {
                return (
                    <TooltipHost content={item['Title']} aria-label="Title">
                        <div
                            style={{
                                display: 'flex',
                                justifyContent: 'left',
                                alignItems: 'center',
                                flexWrap: 'nowrap',
                                height: '100%'
                            }}
                        >
                            <span
                                style={{
                                    fontSize: '12px',
                                    color: TextColors.lightPrimary,
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis'
                                }}
                            >
                                {item['Title']}
                            </span>
                        </div>
                    </TooltipHost>
                );
            }
        },
        {
            key: 'ActionTaken',
            name: 'Action Taken',
            type: 'status',
            fieldName: HistoryRecordFieldNames.ActionTaken,
            isResizable: true,
            maxWidth: 160,
            status: x => {
                switch (true) {
                    case /den/.test(x.ActionTaken.toLowerCase()):
                        return 'AlertDoNotDisturb';
                    case /rej/.test(x.ActionTaken.toLowerCase()):
                        return 'AlertDoNotDisturb';
                    case /appr/.test(x.ActionTaken.toLowerCase()):
                        return 'Positive1';
                    case /comp/.test(x.ActionTaken.toLowerCase()):
                        return 'Positive1';
                    default:
                        return undefined;
                }
            }
        },
        {
            key: 'UnitValue',
            name: 'Unit Value',
            type: 'custom',
            fieldName: HistoryRecordFieldNames.UnitValue,
            isResizable: true,
            maxWidth: 140,
            onRender: (item: any) => {
                return (
                    <TooltipHost content={item['UnitValue'] + ' ' + item['AmountUnits']} aria-label="Amount">
                        <div
                            style={{
                                display: 'flex',
                                justifyContent: 'left',
                                alignItems: 'center',
                                flexWrap: 'nowrap',
                                height: '100%'
                            }}
                        >
                            <span
                                style={{
                                    fontSize: '12px',
                                    color: TextColors.lightPrimary,
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis'
                                }}
                            >
                                {item['UnitValue'] + ' ' + item['AmountUnits']}
                            </span>
                        </div>
                    </TooltipHost>
                );
            }
        },
        {
            key: 'CustomAttribute',
            name: 'Additional Information',
            type: 'text',
            fieldName: HistoryRecordFieldNames.CustomAttribute,
            isResizable: true,
            maxWidth: 200,
            ariaLabel: 'Additional Information',
            onRender: (item: any) => {
                return (
                    <TooltipHost content={item['CustomAttribute']} aria-label="Custom Attribute">
                        <div
                            style={{
                                display: 'flex',
                                justifyContent: 'left',
                                alignItems: 'center',
                                flexWrap: 'nowrap',
                                height: '100%'
                            }}
                        >
                            <span
                                style={{ fontSize: '12px', color: TextColors.lightPrimary, textOverflow: 'ellipsis' }}
                            >
                                {item['CustomAttribute']}
                            </span>
                        </div>
                    </TooltipHost>
                );
            }
        }
    ];

    const tenantToColumnMapping: Dictionary<string[]> = {
        Default: [
            HistoryRecordFieldNames.ActionDate,
            HistoryRecordFieldNames.ActionTaken,
            HistoryRecordFieldNames.Tenant,
            HistoryRecordFieldNames.CustomAttribute,
            HistoryRecordFieldNames.DocumentNumber,
            HistoryRecordFieldNames.SubmitterName,
            HistoryRecordFieldNames.Title,
            HistoryRecordFieldNames.UnitValue
        ],
        Mobile: [HistoryRecordFieldNames.ActionTaken, HistoryRecordFieldNames.DocumentNumber]
    };

    function renderColumns(appName: string) {
        if (windowWidth < 640) {
            return allColumns.filter(column => tenantToColumnMapping['Mobile'].includes(column.fieldName));
        }
        switch (appName) {
            default:
                return allColumns.filter(column => tenantToColumnMapping['Default'].includes(column.fieldName));
        }
    }

    function renderDataGridLabel() {
        let label = '';
        if (historySearchCriteria) {
            label += 'filtered by keyword search:' + historySearchCriteria + '. ';
        }
        let sort = 'Ascending';
        if (sortDirection === 'DESC') {
            sort = 'Descending';
        }
        label +=
            'Showing ' +
            historyData.length.toString() +
            ' out of ' +
            historyTotalRecords +
            ' Approval Records sorted ' +
            sort +
            ' by ' +
            sortColumnField +
            '. Row 1 Contains the columns you can filter by. The following rows each contain a historical record. You can view more details by selecting Request Number. ';
        return label;
    }
    React.useEffect(() => {
        if (
            historySelectedPage === undefined ||
            sortColumnField === undefined ||
            sortDirection === undefined ||
            firstUpdate.current == true
        ) {
            return;
        }
        if (firstUpdate.current) {
            firstUpdate.current = false;
            return;
        }
        let historyGroupSearchCriteria = '';
        if (historyGroupedBy != 'Default') {
            historyGroupSearchCriteria = historyGroupedBy;
        }
        dispatch(
            requestMyHistory(
                historySelectedPage,
                sortColumnField,
                sortDirection,
                historyGroupSearchCriteria,
                historyTimePeriod,
                historyTenantIdFilter
            )
        );
    }, [historyGroupedBy]);

    function filterHistory(history: any): any {
        if (filterValue === 'All') {
            return history;
        }
        return history.filter((historyRecord: any) => historyRecord[historyGroupedBy] === filterValue);
    }
    return (
        <DetailsList
            ariaLabel="Approval Records"
            columns={renderColumns(historyGroupedBy)}
            items={filterHistory(historyData)}
            selectionMode={SelectionMode.none}
            isScrollable={false}
            telemetryHook={telemetryClient}
            onColumnHeaderClick={onSort as any}
            sortByDefault={false}
            ariaLabelForGrid={renderDataGridLabel()}
            onRenderDetailsHeader={onRenderDetailsHeader as any}
        />
    );
}
const connected = withContext(HistoryColumns);
export { connected as HistoryColumns };
