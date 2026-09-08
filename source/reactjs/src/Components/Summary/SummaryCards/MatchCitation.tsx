import * as React from 'react';
import { FontIcon, Icon } from '@fluentui/react';
import { Callout, DirectionalHint } from '@fluentui/react/lib/Callout';
import { getFileTypeIconProps } from '@fluentui/react-file-type-icons';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import * as Styled from './MatchCitation.styled';
import { IMatchCitationProps, IAttachmentMatch } from './MatchCitation.types';
import { trackBusinessProcessEvent, TrackingEventId } from '../../../Helpers/telemetryHelpers';
import { getFileExtension, renderSnippet } from '../../../Helpers/searchHighlightUtils';

type CitationTarget = 'field' | 'attachment-preview' | 'attachment-open' | 'callout-toggle';

function MatchCitationBase(props: IMatchCitationProps): React.ReactElement {
    const { attachmentMatches = [], hasFieldMatch = false, onPreviewClick, onOpenRequest, documentNumber } = props;
    const [isCalloutVisible, setIsCalloutVisible] = React.useState(false);
    const indicatorRef = React.useRef<HTMLButtonElement>(null);
    const calloutId = React.useMemo(() => `match-citation-callout-${Math.random().toString(36).slice(2, 10)}`, []);
    const { telemetryClient, authClient } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    const hasAttachments = attachmentMatches.length > 0;
    if (!hasFieldMatch && !hasAttachments) return null;

    const isSingleAttachment = !hasFieldMatch && attachmentMatches.length === 1;
    const totalLocations = (hasFieldMatch ? 1 : 0) + attachmentMatches.length;
    const showCallout = totalLocations > 1;

    const emitCitationClick = (target: CitationTarget, match?: IAttachmentMatch): void => {
        trackBusinessProcessEvent(
            authClient,
            telemetryClient,
            'DeepSearch - Citation clicked',
            'MSApprovals.DeepSearch.CitationClick',
            TrackingEventId.DeepSearchCitationClick,
            {},
            {
                Target: target,
                TotalLocations: totalLocations,
                HasFieldMatch: hasFieldMatch,
                AttachmentCount: attachmentMatches.length,
                ...(match?.AttachmentId != null && { AttachmentId: match.AttachmentId }),
                ...(match?.AttachmentName && { AttachmentName: match.AttachmentName }),
                ...(documentNumber && { DocumentNumber: documentNumber }),
            }
        );
    };

    // Unified click handler for all citation actions
    const handleAction =
        (target: CitationTarget, match?: IAttachmentMatch) =>
        (e: React.MouseEvent): void => {
            e.stopPropagation();
            setIsCalloutVisible(false);
            emitCitationClick(target, match);
            if (match?.AttachmentId != null) {
                onPreviewClick(match.AttachmentId, match.AttachmentName);
            } else if (target !== 'callout-toggle') {
                onOpenRequest?.();
            }
        };

    const handleIndicatorClick = (e: React.MouseEvent): void => {
        e.stopPropagation();
        if (isSingleAttachment && attachmentMatches[0].AttachmentId != null) {
            handleAction('attachment-preview', attachmentMatches[0])(e);
            return;
        }
        if (!showCallout) {
            handleAction('field')(e);
            return;
        }
        emitCitationClick('callout-toggle');
        setIsCalloutVisible(!isCalloutVisible);
    };

    // Computed indicator props
    const singleAttachmentClickable = isSingleAttachment && attachmentMatches[0].AttachmentId != null;
    const labelText = !hasAttachments
        ? 'Found in request details'
        : isSingleAttachment
        ? (<>Found in: <Styled.AttachmentLink>{attachmentMatches[0].AttachmentName}</Styled.AttachmentLink></>)
        : `Found in ${totalLocations} locations`;
    const titleText = !hasAttachments
        ? 'Click to open request — found in request details'
        : singleAttachmentClickable
        ? `Click to preview ${attachmentMatches[0].AttachmentName}`
        : isSingleAttachment
        ? `Click to open request — found in ${attachmentMatches[0].AttachmentName}`
        : `Click to see all ${totalLocations} match locations`;
    const ariaLabel = !hasAttachments
        ? 'Found in request details. Click to open request.'
        : `${totalLocations} match locations found. Click to see details.`;
    const trailingIconName = showCallout ? 'ChevronDown' : singleAttachmentClickable ? 'DocumentSearch' : 'ChevronRight';
    const trailingIconSize = trailingIconName === 'DocumentSearch' ? 12 : 10;

    return (
        <>
            <Styled.CompactIndicator
                ref={showCallout ? indicatorRef : undefined}
                onClick={handleIndicatorClick}
                aria-label={ariaLabel}
                title={titleText}
                aria-haspopup={showCallout || singleAttachmentClickable ? 'dialog' : undefined}
                aria-expanded={showCallout ? isCalloutVisible : undefined}
                aria-controls={showCallout ? calloutId : undefined}
            >
                <Styled.IndicatorText>{labelText}</Styled.IndicatorText>
                <Styled.TrailingIcon>
                    <Icon iconName={trailingIconName} style={{ fontSize: trailingIconSize }} />
                </Styled.TrailingIcon>
            </Styled.CompactIndicator>
            {showCallout && isCalloutVisible && indicatorRef.current && (
                <Callout
                    target={indicatorRef.current}
                    onDismiss={() => setIsCalloutVisible(false)}
                    directionalHint={DirectionalHint.bottomLeftEdge}
                    setInitialFocus
                    isBeakVisible={false}
                    gapSpace={4}
                    calloutMaxWidth={360}
                    role="dialog"
                    aria-label={`Match locations for ${documentNumber || 'this request'}`}
                    id={calloutId}
                    styles={{ calloutMain: { padding: '8px 0', minWidth: 'min(240px, 90vw)' } }}
                >
                    {hasFieldMatch && (
                        <Styled.CalloutItem
                            aria-label="Open request — match found in request details"
                            onClick={handleAction('field')}
                        >
                            <Styled.CalloutItemIcon>
                                <Icon iconName="TextDocument" style={{ fontSize: 16 }} />
                            </Styled.CalloutItemIcon>
                            <Styled.CalloutItemContent>
                                <Styled.CalloutItemName>Request details</Styled.CalloutItemName>
                            </Styled.CalloutItemContent>
                            <Styled.TrailingIcon>
                                <Icon iconName="ChevronRight" style={{ fontSize: 10 }} />
                            </Styled.TrailingIcon>
                        </Styled.CalloutItem>
                    )}
                    {attachmentMatches.map((match, index) => {
                        const isClickable = match.AttachmentId != null;
                        const snippet = renderSnippet(match.Snippet);
                        const target: CitationTarget = isClickable ? 'attachment-preview' : 'attachment-open';
                        return (
                            <Styled.CalloutItem
                                key={match.AttachmentId || `attach-${index}`}
                                onClick={handleAction(target, match)}
                                aria-label={isClickable ? `Preview ${match.AttachmentName}` : match.AttachmentName}
                            >
                                <Styled.CalloutItemIcon>
                                    <FontIcon
                                        {...getFileTypeIconProps({
                                            extension: getFileExtension(match.AttachmentName),
                                            size: 16,
                                            imageFileType: 'svg',
                                        })}
                                    />
                                </Styled.CalloutItemIcon>
                                <Styled.CalloutItemContent>
                                    {isClickable ? (
                                        <Styled.AttachmentLink>{match.AttachmentName}</Styled.AttachmentLink>
                                    ) : (
                                        <Styled.CalloutItemName>{match.AttachmentName}</Styled.CalloutItemName>
                                    )}
                                    {snippet && <Styled.CalloutItemSnippet>{snippet}</Styled.CalloutItemSnippet>}
                                </Styled.CalloutItemContent>
                                <Styled.TrailingIcon>
                                    <Icon
                                        iconName={isClickable ? 'DocumentSearch' : 'ChevronRight'}
                                        style={{ fontSize: isClickable ? 12 : 10 }}
                                    />
                                </Styled.TrailingIcon>
                            </Styled.CalloutItem>
                        );
                    })}
                </Callout>
            )}
        </>
    );
}

export const MatchCitation = React.memo(MatchCitationBase);
