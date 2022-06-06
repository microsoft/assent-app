import * as React from 'react';
import * as sanitizeHtml from 'sanitize-html';
import { Icon } from '@fluentui/react/lib/Icon';
import * as notificationStyled from './NotificationsPanelStyling';
import { NProps } from './NotificationsPanel.types';

export function RenderListItem(props: NProps): React.ReactElement {
    const { item, status } = props;
    const [messageTigger, setMessageTrigger] = React.useState(false);
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

    if (status === 'unread') {
        return (
            <div
                className={notificationStyled.classNames.itemCellBold}
                data-is-focusable={true}
                onClick={() => {
                    setMessageTrigger(!messageTigger);
                }}
            >
                <Icon iconName={item.subjectIcon} className={subjectIcon(item.subjectIcon)} />
                <div className={notificationStyled.classNames.itemContent}>
                    <div className={notificationStyled.classNames.itemNameBold}>{item.subjectHeader}</div>
                    {messageTigger && (
                        <div className={notificationStyled.classNames.itemMessageBold}>
                            <div dangerouslySetInnerHTML={{ __html: sanitizeMessageBodyText }} />
                        </div>
                    )}
                </div>
                <Icon
                    className={notificationStyled.classNames.chevron}
                    iconName={!messageTigger ? 'ChevronRight' : 'ChevronDown'}
                />
            </div>
        );
    } else {
        return (
            <div
                className={notificationStyled.classNames.itemCell}
                data-is-focusable={true}
                onClick={() => {
                    setMessageTrigger(!messageTigger);
                }}
            >
                <Icon iconName={item.subjectIcon} className={subjectIcon(item.subjectIcon)} />
                <div className={notificationStyled.classNames.itemContent}>
                    <div className={notificationStyled.classNames.itemName}>{item.subjectHeader}</div>
                    {messageTigger && (
                        <div className={notificationStyled.classNames.itemMessage}>
                            <div dangerouslySetInnerHTML={{ __html: sanitizeMessageBodyText }} />
                        </div>
                    )}
                </div>
                <Icon
                    className={notificationStyled.classNames.chevron}
                    iconName={!messageTigger ? 'ChevronRight' : 'ChevronDown'}
                />
            </div>
        );
    }
}
