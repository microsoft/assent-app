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
import { imitateClickOnKeyPressForDiv, formatUnitValue } from '../../../Helpers/sharedHelpers';
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
    getIsSearchResultsViewOpen,
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
import { useHistory, useLocation } from 'react-router-dom';
import { NavigationUtils } from '../../Shared/Utils/NavigationUtils';
import { MatchCitation } from './MatchCitation';
import { hasFieldHighlights } from '../../../Helpers/searchHighlightUtils';

// Returns the summary item's AdditionalData override bag when the tenant's MetadataExtractionMapping is Enabled, else null.
function getMappedAdditionalData(tenantInfo: any, summary: any, tenantId: any, documentNumber: string) {
    try {
        const tenant = tenantInfo?.find?.((t: any) => t.tenantId === Number(tenantId));
        if (tenant?.metadataExtractionMapping && JSON.parse(tenant.metadataExtractionMapping)?.Enabled === true) {
            const raw = summary?.find?.((i: any) => i.ApprovalIdentifier?.DocumentNumber === documentNumber);
            return raw?.AdditionalData ?? null;
        }
    } catch {
        /* invalid mapping — fall back to original fields */
    }
    return null;
}

function SummaryCardBase(props: ISummaryCardProps): React.ReactElement {
    let {
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
    let cardRef = props.cardRef;
    const formattedDate = new Date(SubmittedDate).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION); // MMM DD,YYYY
    const { useSelector, dispatch, telemetryClient, authClient } = React.useContext(
        Context as React.Context<IEmployeeExperienceContext>
    );
    const summaryGroupedBy = useSelector(getSummaryGroupedBy);
    const tenantInfo = useSelector(getTenantInfo);
    const summary = useSelector(getSummary);
    // Prefer AdditionalData._* overrides when present, else the original fields (nav/state/telemetry still use originals).
    const mappedAdditionalData = getMappedAdditionalData(tenantInfo, summary, TenantId, DocumentNumber);
    const displayTitle = mappedAdditionalData?._title ?? Title;
    const displayCustomAttributeValue = mappedAdditionalData?._customAttributeValue ?? CustomAttributeValue;
    const effectiveUnitValue = mappedAdditionalData?._unitValue ?? UnitValue;
    const effectiveUnitofMeasure = mappedAdditionalData?._unitOfMeasure ?? UnitofMeasure;
    const displayDocumentNumberLabel = mappedAdditionalData?._displayDocumentNumber ?? DisplayDocumentNumber;
    const formattedUnitValue = formatUnitValue(effectiveUnitValue);
    const selectedApprovalRecords = useSelector(getSelectedApprovalRecords);
    const isBulkSelected = useSelector(getIsBulkSelected);
    const failedRequests = useSelector(getFailedRequests);
    const disabled = useSelector(getIsDisabled);
    const summaryCommonProperties = useSelector(getSummaryCommonPropertiesSelector);
    const bulkActionConcurrentCall = useSelector(getBulkActionConcurrentCall);
    const isSearchResultsViewOpen = useSelector(getIsSearchResultsViewOpen);
    const attachmentMatches = props.cardInfo._matchMetadata?.attachmentMatches;
    const hasFieldMatch = hasFieldHighlights(props.cardInfo._matchMetadata?.highlights);
    const handleAttachmentPreview = (attachmentId: string | null, attachmentName: string) => {
        if (attachmentId && props.onSearchPreviewClick) {
            props.onSearchPreviewClick(
                TenantId?.toString(),
                DocumentNumber,
                DisplayDocumentNumber,
                attachmentId,
                attachmentName
            );
        }
    };
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
        const isSelectionEnabled = isBulkSelected && isCompliant && (!isMaxSelected || checked) && props.isCardAvailableForBulk && !isSearchResultsViewOpen;
        let a11yTitle = DisplayDocumentNumber + " Checkbox for bulk approval";

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
                    <div style={emptyFailedIconStyle} />{' '}
                    <span role="img" aria-hidden={true}>
                        <Styled.MailReadIcon title="Read Request Icon" aria-hidden={true} />
                    </span>{' '}
                </>
            );
        } else if (!isSelectionEnabled && !isReadLocal) {
            icon = (
                <>
                    <div style={emptyFailedIconStyle} />{' '}
                    <span role="img" aria-hidden={true}>
                        <Styled.MailUnreadIcon title="Unread Request Icon" aria-hidden={true} />
                    </span>{' '}
                </>
            );
        } else if (isSelectionEnabled && !lastFailedLocal) {
            icon = (
                <>
                    <div style={emptyFailedIconStyle} />{' '}
                    <Checkbox
                        checked={checked}
                        title={a11yTitle}
                        ariaLabel={a11yTitle}
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
                        title={a11yTitle}
                        ariaLabel={a11yTitle}
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
    if (CustomAttributeName || displayCustomAttributeValue) {
        isCustomAttributeVisible = true;
    }

    const isValidTenantIdAndDocumentNumber = (): boolean => {
        let isTenantIdNotNull = TenantId !== null;
        let isTenantIdNotUndefined = TenantId !== undefined;
        let isDocumentNumberNotNull = DocumentNumber !== null;
        let isDocumentNumberNotUndefined = DocumentNumber !== undefined;
        let isTenantId = isTenantIdNotNull && isTenantIdNotUndefined;
        let isDocumentNumber = isDocumentNumberNotNull && isDocumentNumberNotUndefined;

        return isTenantId && isDocumentNumber;
    };

    const history = useHistory();
    function handleClickwithLogging(ev: any): void {
        const isCheckBoxClicked =
            ev.target.getAttribute('class')?.indexOf('Checkbox') > -1 ||
            ev.target.getAttribute('type') == 'checkbox' ||
            ev.target.getAttribute('data-icon-name') == 'CheckMark'
                ? true
                : false;
        if (!isCheckBoxClicked && isValidTenantIdAndDocumentNumber()) {
            // Navigate to details using NavigationUtils
            NavigationUtils.navigateToDetails(history, TenantId, DisplayDocumentNumber, true, true);

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
            case GroupingBy.Category:
                header = AppName;
                break;
            default:
                header = Submitter;
        }
        return (
            <div tabIndex={0} role="heading" aria-level={4}>
                <TooltipHost content={header}>{header}</TooltipHost>
            </div>
        );
    }

    const hasCitation =
        isSearchResultsViewOpen && (hasFieldMatch || (attachmentMatches && attachmentMatches.length > 0));

    const readStatus = lastFailedLocal ? 'Error' : isReadLocal ? 'Read' : 'Unread';

    return (
        <Styled.CardWrapper>
            <Styled.Card
                id={isBulkSelected ? 'summaryCard' + DisplayDocumentNumber : cardRef}
                lastFailed={lastFailedLocal}
                onClick={isBulkSelected ? null : handleClickwithLogging}
                role={isBulkSelected ? null : 'button'}
                className={getBulkClassName()}
                tabIndex={disabled || isBulkSelected ? null : 0}
                onKeyPress={
                    disabled || isBulkSelected ? null : imitateClickOnKeyPressForDiv(() => detailCallPerf(cardRef))
                }
                footer={summaryGroupedBy}
                isRead={isReadLocal}
                isSelected={isSelectedInState}
            >
                <Styled.CardHeader>
                    <Styled.Header>
                        {(summaryGroupedBy == GroupingBy.Tenant || summaryGroupedBy == GroupingBy.Date) && (
                            <SubmitterPersona emailAlias={SubmitterAlias} size={PersonaSize.size32} />
                        )}
                        {(summaryGroupedBy == GroupingBy.Submitter || summaryGroupedBy == GroupingBy.Category) && (
                            <Styled.HeaderTenantIcon>
                                {getTenantIcon(AppName, tenantInfo, '24px')}
                            </Styled.HeaderTenantIcon>
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
                            <Text tooltipMaxWidth={200}>{displayTitle}</Text>{' '}
                        </Styled.Title>
                        <Styled.SecondaryTitleContainer>
                            {isCustomAttributeVisible && <Text> {displayCustomAttributeValue} </Text>}
                        </Styled.SecondaryTitleContainer>
                        <Styled.UnitValueRow>
                            {effectiveUnitValue && (
                                <Styled.UnitValue isRead={isReadLocal}>
                                    <Text tooltipMaxWidth={200}>{formattedUnitValue}</Text>
                                </Styled.UnitValue>
                            )}
                            {effectiveUnitofMeasure && effectiveUnitofMeasure !== '-' && (
                                <Styled.UnitofMeasure isRead={isReadLocal}>
                                    <Text tooltipMaxWidth={200}>{effectiveUnitofMeasure}</Text>
                                </Styled.UnitofMeasure>
                            )}
                        </Styled.UnitValueRow>
                        {DisplayDocumentNumber && (
                            <Styled.DisplayDocumentNumber isRead={isReadLocal}>
                                <Text tooltipMaxWidth={200}>{displayDocumentNumberLabel}</Text>
                            </Styled.DisplayDocumentNumber>
                        )}
                        <Styled.DateRow>
                            {formattedDate && <Styled.Date> {formattedDate}</Styled.Date>}
                            {CompanyCode && <Styled.CompanyCode>{CompanyCode}</Styled.CompanyCode>}
                        </Styled.DateRow>
                        <span className="sr-only">{readStatus}</span>
                    </Styled.TextContainer>
                </Styled.CardBody>
            </Styled.Card>
            {hasCitation && (
                <MatchCitation
                    attachmentMatches={attachmentMatches || []}
                    hasFieldMatch={hasFieldMatch}
                    onPreviewClick={handleAttachmentPreview}
                    onOpenRequest={() => detailCallPerf(cardRef)}
                    documentNumber={DocumentNumber}
                />
            )}
        </Styled.CardWrapper>
    );
}

const memoizedSummaryView = React.memo(SummaryCardBase);
export { memoizedSummaryView as SummaryCard };
