import { IconButton, Modal, mergeStyleSets } from '@fluentui/react';
import * as React from 'react';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { getIsSettingPanelOpen } from '../Shared/SharedComponents.selectors';
import { UserSettings, MOBILE_QUERY, SETTINGS_SECTIONS } from '../Shared/Components/UserSettings';
import { requestQuickTourInfo, toggleSettingsPanel } from '../Shared/SharedComponents.actions';

const modalStyles = mergeStyleSets({
    container: {
        width: '720px',
        height: '540px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        display: 'flex',
        flexDirection: 'column',
        borderRadius: '8px',
        overflow: 'hidden',
        backgroundColor: '#FFFFFF',
    },
    scrollableContent: {
        flex: 1,
        minHeight: 0,
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
    },
    header: {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '14px 20px',
        borderBottom: '1px solid #EDEBE9',
        backgroundColor: '#FFFFFF',
        flexShrink: 0,
    },
    title: {
        fontSize: '16px',
        fontWeight: 600,
        color: '#242424',
        margin: 0,
    },
    body: {
        flex: 1,
        minHeight: 0,
        display: 'flex',
    },
});

export function UserSettingsPanel(): React.ReactElement {
    const { useSelector, dispatch } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);
    const isSettingPanelOpen = useSelector(getIsSettingPanelOpen);

    const [section, setSection] = React.useState('general');
    const [isMobile, setIsMobile] = React.useState(
        typeof window !== 'undefined' && window.matchMedia(MOBILE_QUERY).matches
    );
    const [drilledIn, setDrilledIn] = React.useState(false);

    React.useEffect(() => {
        const mql = window.matchMedia(MOBILE_QUERY);
        const onChange = (e: MediaQueryListEvent) => {
            setIsMobile(e.matches);
            if (!e.matches) setDrilledIn(false);
        };
        mql.addEventListener('change', onChange);
        return () => mql.removeEventListener('change', onChange);
    }, []);

    const handleSettingsDismiss = (): void => {
        dispatch(toggleSettingsPanel(false));
        setDrilledIn(false);
        // Re-sync coachmark/pulse state from backend after the user closes the panel.
        dispatch(requestQuickTourInfo());
    };

    const showBack = isMobile && drilledIn;
    const headerText = showBack
        ? SETTINGS_SECTIONS.find((s) => s.key === section)?.name ?? 'Settings'
        : 'Settings';

    return (
        <Modal
            isOpen={isSettingPanelOpen}
            onDismiss={handleSettingsDismiss}
            isBlocking={false}
            allowTouchBodyScroll
            containerClassName={modalStyles.container}
            scrollableContentClassName={modalStyles.scrollableContent}
            titleAriaId="user-settings-title"
        >
            <div className={modalStyles.header}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 4, minWidth: 0 }}>
                    {showBack && (
                        <IconButton
                            iconProps={{ iconName: 'ChevronLeft' }}
                            ariaLabel="Back to settings list"
                            onClick={() => setDrilledIn(false)}
                        />
                    )}
                    <h2 id="user-settings-title" className={modalStyles.title}>
                        {headerText}
                    </h2>
                </div>
                <IconButton
                    iconProps={{ iconName: 'Cancel' }}
                    ariaLabel="Close settings"
                    onClick={handleSettingsDismiss}
                />
            </div>
            <div className={modalStyles.body}>
                <UserSettings
                    isMobile={isMobile}
                    drilledIn={drilledIn}
                    section={section}
                    onSelectSection={(key) => {
                        setSection(key);
                        if (isMobile) setDrilledIn(true);
                    }}
                />
            </div>
        </Modal>
    );
}
