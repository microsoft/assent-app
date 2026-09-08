import * as React from 'react';
import * as sanitizeHtml from 'sanitize-html';
import { Icon } from '@fluentui/react/lib/Icon';
import * as notificationStyled from './NotificationsPanelStyling';
import { NProps } from './NotificationsPanel.types';

export function RenderListItem(props: NProps): React.ReactElement {
    const { item, status } = props;
    const [messageTrigger, setMessageTrigger] = React.useState(false);
    const sanitizeMessageBodyText = sanitizeHtml(item.messageBodyText, {
        allowedTags: ['a', 'p', 'strong'],
        allowedAttributes: {
            a: ['href', 'target'],
        },
    });

    React.useEffect(() => {
        if (status === 'unread') {
            setMessageTrigger(true);
        }
    }, [status]);

    const subjectIcon = (iconType: string) => {
        if (iconType === 'ReportWarning') {
            return notificationStyled.ReportWarning;
        } else if (iconType === 'InfoSolid') {
            return notificationStyled.InfoSolid;
        } else if (iconType === 'IncidentTriangle') {
            return notificationStyled.IncidentTriangle;
        }
    };

    // Handle key down event for keyboard accessibility
    const handleKeyDown = (event: React.KeyboardEvent): void => {
        if (event.key === 'Enter' || event.key === ' ' || event.key === 'Spacebar') {
            event.preventDefault();
            setMessageTrigger(!messageTrigger);
        }
    };

    const expandCollapseText = messageTrigger ? 'Collapse notification details' : 'Expand notification details';
    const isUnread = status === 'unread';
    const cellClassName = isUnread ? notificationStyled.classNames.itemCellBold : notificationStyled.classNames.itemCell;
    const headerClassName = isUnread ? notificationStyled.classNames.itemNameBold : notificationStyled.classNames.itemName;
    const messageClassName = isUnread ? notificationStyled.classNames.itemMessageBold : notificationStyled.classNames.itemMessage;
    const a11yClassName = 'msapprovals_a11y_notification';
    return (
        <div
            className={cellClassName + " " + a11yClassName}
            tabIndex={0}
            data-is-focusable={true}
            role="button"
            aria-expanded={messageTrigger}
            aria-label={`${item.subjectHeader} notification, ${expandCollapseText}`}
            onClick={() => {
                setMessageTrigger(!messageTrigger);
            }}
            onKeyDown={handleKeyDown}
        >
            <Icon iconName={item.subjectIcon} className={subjectIcon(item.subjectIcon)} />
            <div className={notificationStyled.classNames.itemContent}>
                <div className={headerClassName}>{item.subjectHeader}</div>
                {messageTrigger && (
                    <div className={messageClassName}>
                        <div dangerouslySetInnerHTML={{ __html: sanitizeMessageBodyText }} />
                    </div>
                )}
            </div>
            <Icon
                className={notificationStyled.classNames.chevron}
                iconName={!messageTrigger ? 'ChevronRight' : 'ChevronDown'}
                aria-hidden="true"
            />
        </div>
    );
}
