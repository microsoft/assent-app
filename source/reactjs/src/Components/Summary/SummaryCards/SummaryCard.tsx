import * as React from 'react';
import * as Styled from './SummaryCard.styled';
import { ISummaryCardProps } from './SummaryCard.types';
import { Icon } from '@fluentui/react/lib/Icon';
import { Checkbox } from '@fluentui/react/lib/Checkbox';
import Text from 'react-texty';
import 'react-texty/styles.css';
import { Styles, failedIconStyle, emptyFailedIconStyle } from './SummaryCard.styled';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';

import { updateApprovalRecords } from '../../Shared/SharedComponents.actions';
import { imitateClickOnKeyPressForDiv } from '../../../Helpers/sharedHelpers';
import { PersonaSize } from '../../Shared/Components/Persona/Persona.types';
import { getTenantIcon } from '../../Shared/Components/IconMapping';
import { mapDate } from '../../Shared/Components/DateFormatting';
import {
    getSummaryCommonPropertiesSelector,
    getSummaryGroupedBy,
    getTenantInfo,
    getSummary,
    getIsBulkSelected,
    getBulkActionConcurrentCall,
    getSelectedSummaryTileRef,
    getPanelOpen,
    getSelectedApprovalRecords,
} from '../../Shared/SharedComponents.selectors';
import { trackBusinessProcessEvent, TrackingEventId } from '../../../Helpers/telemetryHelpers';
import { GroupingBy } from '../../Shared/Components/GroupingBy';
import { TooltipHost } from '@fluentui/react';
import { setSelectedSumaryTileRef, updatePanelState } from '../../Shared/SharedComponents.actions';
import { updateMyRequest } from '../../Shared/Details/Details.actions';
import { SubmitterPersona } from '../../Shared/Components/SubmitterPersona';
import {
    getFailedRequests,
    getIsDisabled,
    isRequestCurrentlySelected,
    isRequestRead,
} from '../../Shared/Details/Details.selectors';
import { DATE_FORMAT_OPTION, DEFAULT_LOCALE } from '../../Shared/SharedConstants';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';

function SummaryCardBase(props: ISummaryCardProps): React.ReactElement {
    const {
        SubmittedDate,
        Title,
        CompanyCode,
        UnitValue,
        UnitofMeasure,
        DisplayDocumentNumber,
        DocumentNumber,
        FiscalYear,
        isRead,
        lastFailed,
        Submitter,
        SubmitterAlias,
        TenantId,
        AppName,
        CustomAttributeName,
        CustomAttributeValue,
        IsControlsAndComplianceRequired,
    } = props.cardInfo;
    const cardRef = props.cardRef;
    const formattedDate = new Date(SubmittedDate).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION); // MMM DD,YYYY
    const formattedUnitValue = isNaN(Number(UnitValue))
        ? UnitValue
        : Number(UnitValue).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const { useSelector, dispatch, telemetryClient, authClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const summaryGroupedBy = useSelector(getSummaryGroupedBy);
    const tenantInfo = useSelector(getTenantInfo);
    const summary = useSelector(getSummary);
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
    const isBulkSelected = useSelector(getIsBulkSelected);
    const failedRequests = useSelector(getFailedRequests);
    const disabled = useSelector(getIsDisabled);
    const summaryCommonProperties = useSelector(getSummaryCommonPropertiesSelector);
    const bulkActionConcurrentCall = useSelector(getBulkActionConcurrentCall);
    const isReadInState = useSelector((state: any) => isRequestRead(state, DisplayDocumentNumber));
    const isSelectedInState = useSelector((state: any) => isRequestCurrentlySelected(state, DocumentNumber));

    const isReadLocal = isRead || isReadInState;
    const lastFailedLocal = lastFailed || failedRequests.includes(DisplayDocumentNumber);

    const selectRecords = (ev: React.FormEvent<HTMLElement>, checked: boolean): void => {
        handleBulkApprovalRecords(checked);
    };

    React.useEffect(() => {
        if (isBulkSelected && props.allBulkCheckSelected) {
            if (props.selectForBulkApproval) {
                handleBulkApprovalRecords(props.selectForBulkApproval);
            }
        }
    }, [props.allBulkCheckSelected]);

    function handleBulkApprovalRecords(selected: boolean): void {
        if (selected) {
            const allSummaryList = summary;
            const tempArray = allSummaryList.filter(
                (item: { ApprovalIdentifier: { DocumentNumber: string } }) =>
                    item.ApprovalIdentifier.DocumentNumber === DocumentNumber
            );
            tempArray[0].cardRef = cardRef;
            const approvalIdentifier = tempArray[0].ApprovalIdentifier;
            if (!IsCardSelectedForBulkApproval()) {
                dispatch(updateApprovalRecords(approvalIdentifier, 'Push'));
            }
        } else {
            const filteredApprovalRecords = selectedApprovalRecords.filter(
                (item: { DocumentNumber: string }) => item.DocumentNumber != DocumentNumber
            );
            dispatch(updateApprovalRecords(filteredApprovalRecords));
        }
    }

    function getBulkClassName(): string {
        return IsCardSelectedForBulkApproval() ? 'bulkSelectedCardStyle' : 'defaultCardStyle';
    }

    function detailCallPerf(currentRef: string): void {
        if (!isSelectedInState) {
            dispatch(updateMyRequest(Number(TenantId), DocumentNumber, DisplayDocumentNumber, FiscalYear));
            dispatch(setSelectedSumaryTileRef(currentRef));
            dispatch(updatePanelState(true));
        }
    }

    function IsCardSelectedForBulkApproval(): boolean {
        for (let i = 0; i < selectedApprovalRecords.length; i++) {
            if (selectedApprovalRecords[i].DocumentNumber == DocumentNumber) {
                return true;
            }
        }
        return false;
    }

    function renderIcon() {
        let checkBoxRef = cardRef + 'check';
        let checked = IsCardSelectedForBulkApproval();
        const isMaxSelected = isBulkSelected && selectedApprovalRecords.length >= bulkActionConcurrentCall;
        const isCompliant = !IsControlsAndComplianceRequired || isReadLocal;
        const isSelectionEnabled = isBulkSelected && isCompliant && (!isMaxSelected || checked);
        let icon;
        if (lastFailedLocal && !isBulkSelected) {
            icon = (
                <>
                    <div style={emptyFailedIconStyle} />{' '}
                    <Icon iconName="ReportWarning" title="Report Warning" style={failedIconStyle} />
                </>
            );
        } else if (!isSelectionEnabled && isReadLocal) {
            icon = (
                <>
                    <div style={emptyFailedIconStyle} /> <Styled.MailReadIcon title="Read Request Icon" />{' '}
                </>
            );
        } else if (!isSelectionEnabled && !isReadLocal) {
            icon = (
                <>
                    <div style={emptyFailedIconStyle} /> <Styled.MailUnreadIcon title="Unread Request Icon" />
                </>
            );
        } else if (isSelectionEnabled && !lastFailedLocal) {
            icon = (
                <>
                    <div style={emptyFailedIconStyle} />{' '}
                    <Checkbox
                        checked={checked}
                        title="Checkbox for bulk approval"
                        ariaLabel="Checkbox for bulk approval"
                        role="checkbox"
                        aria-checked={true}
                        id={checkBoxRef}
                        onChange={selectRecords}
                    />
                </>
            );
        } else if (isSelectionEnabled && lastFailedLocal) {
            icon = (
                <>
                    <Icon iconName="ReportWarning" style={failedIconStyle} />{' '}
                    <Checkbox
                        checked={checked}
                        title="Checkbox for bulk approval"
                        ariaLabel="Checkbox for bulk approval"
                        role="checkbox"
                        aria-checked={true}
                        id={checkBoxRef}
                        onChange={selectRecords}
                    />
                </>
            );
        } else {
            null;
        }
        return icon;
    }

    var isCustomAttributeVisible = false;
    if (CustomAttributeName || CustomAttributeValue) {
        isCustomAttributeVisible = true;
    }

    function handleClickwithLogging(ev: any): void {
        const isCheckBoxClicked =
            ev.target.getAttribute('class')?.indexOf('Checkbox') > -1 ||
            ev.target.getAttribute('type') == 'checkbox' ||
            ev.target.getAttribute('data-icon-name') == 'CheckMark'
                ? true
                : false;
        if (!isCheckBoxClicked) {
            detailCallPerf(cardRef);
            //request specific properties added additionally since they're not stored in the state yet
            trackBusinessProcessEvent(
                authClient,
                telemetryClient,
                'Summary card clicked',
                'MSApprovals.SummaryCardClicked',
                TrackingEventId.SummaryCardClicked,
                summaryCommonProperties,
                {
                    Xcv: DisplayDocumentNumber,
                    DocumentNumber: DocumentNumber,
                    DisplayDocumentNumber: DisplayDocumentNumber,
                    TenantId: TenantId,
                }
            );
        }
    }

    function renderHeaderTitle(): JSX.Element {
        let header: string;
        switch (summaryGroupedBy) {
            case GroupingBy.Tenant:
                header = Submitter;
                break;
            case GroupingBy.Submitter:
                header = AppName;
                break;
            case GroupingBy.Date:
                header = Submitter;
                break;
            default:
                header = Submitter;
        }
        return (
            <TooltipHost content={header} aria-label="Header">
                {header}
            </TooltipHost>
        );
    }

    return (
        <Styled.Card
            id={isBulkSelected ? 'summaryCard' + DisplayDocumentNumber : cardRef}
            lastFailed={lastFailedLocal}
            onClick={isBulkSelected ? null : handleClickwithLogging}
            role={isBulkSelected ? null : 'button'}
            className={getBulkClassName()}
            tabIndex={disabled || isBulkSelected ? null : 0}
            onKeyPress={disabled || isBulkSelected ? null : imitateClickOnKeyPressForDiv(() => detailCallPerf(cardRef))}
            footer={summaryGroupedBy}
            isRead={isReadLocal}
            isSelected={isSelectedInState}
        >
            <Styled.CardHeader>
                <Styled.Header>
                    {(summaryGroupedBy == GroupingBy.Tenant || summaryGroupedBy == GroupingBy.Date) && (
                        <SubmitterPersona emailAlias={SubmitterAlias} size={PersonaSize.size32} />
                    )}
                    {summaryGroupedBy == GroupingBy.Submitter && (
                        <Styled.HeaderTenantIcon>{getTenantIcon(AppName, tenantInfo, '24px')}</Styled.HeaderTenantIcon>
                    )}
                    <Styled.HeaderTitleContainer className={Styles.text} isRead={isReadLocal}>
                        <Styled.StrongHeaderTitle>{renderHeaderTitle()}</Styled.StrongHeaderTitle>
                    </Styled.HeaderTitleContainer>
                    <Styled.HeaderIcons>{renderIcon()}</Styled.HeaderIcons>
                </Styled.Header>
            </Styled.CardHeader>
            <Styled.CardBody
                id={isBulkSelected ? cardRef : 'summaryCardBody' + DisplayDocumentNumber}
                onClick={isBulkSelected ? handleClickwithLogging : null}
                role={isBulkSelected ? 'button' : null}
                tabIndex={disabled || !isBulkSelected ? null : 0}
                onKeyPress={
                    disabled || !isBulkSelected ? null : imitateClickOnKeyPressForDiv(() => detailCallPerf(cardRef))
                }
            >
                <Styled.TextContainer>
                    <Styled.Title>
                        {' '}
                        <Text tooltipMaxWidth={200}>{Title}</Text>{' '}
                    </Styled.Title>
                    <Styled.SecondaryTitleContainer>
                        {isCustomAttributeVisible && <Text> {CustomAttributeValue} </Text>}
                    </Styled.SecondaryTitleContainer>
                    <Styled.UnitValueRow>
                        {UnitValue && (
                            <Styled.UnitValue isRead={isReadLocal}>
                                <Text tooltipMaxWidth={200}>{formattedUnitValue}</Text>
                            </Styled.UnitValue>
                        )}
                        {UnitofMeasure && (
                            <Styled.UnitofMeasure isRead={isReadLocal}>
                                <Text tooltipMaxWidth={200}>{UnitofMeasure}</Text>
                            </Styled.UnitofMeasure>
                        )}
                    </Styled.UnitValueRow>
                    {DisplayDocumentNumber && (
                        <Styled.DisplayDocumentNumber isRead={isReadLocal}>
                            <Text tooltipMaxWidth={200}>{DisplayDocumentNumber}</Text>
                        </Styled.DisplayDocumentNumber>
                    )}
                    <Styled.DateRow>
                        {formattedDate && <Styled.Date> {formattedDate}</Styled.Date>}
                        {CompanyCode && <Styled.CompanyCode>{CompanyCode}</Styled.CompanyCode>}
                    </Styled.DateRow>
                </Styled.TextContainer>
            </Styled.CardBody>
        </Styled.Card>
    );
}

const memoizedSummaryView = React.memo(SummaryCardBase);
export { memoizedSummaryView as SummaryCard };
