import * as React from 'react';
import { Stack } from '@fluentui/react/lib/Stack';
import { IButtonStyles, IconButton } from '@fluentui/react';
import { CommandBarButton } from '@fluentui/react/lib/Button';
import { INavLink } from '../../../navConfig';
import { isMobileResolution } from '../../../Helpers/sharedHelpers';

export function SideNav(props: { links: INavLink[] }): React.ReactElement {
    const { links } = props;
    const [isNavExpanded, setIsNavExpanded] = React.useState(false);

    const [dimensions, setDimensions] = React.useState({
        height: window.innerHeight,
        width: window.innerWidth,
    });

    React.useEffect(() => {
        function handleResize(): void {
            setDimensions({
                height: window.innerHeight,
                width: window.innerWidth,
            });
        }
        window.addEventListener('resize', handleResize);

        return (): void => {
            window.removeEventListener('resize', handleResize);
        };
    }, []);

    const isMobile = isMobileResolution(dimensions.width);

    const navIconStyles: IButtonStyles = {
        root: { height: '40px', marginLeft: '5px', marginBottom: isNavExpanded ? '8px' : '0px' },
        icon: { color: 'black' },
    };

    const navButtonStyles: IButtonStyles = {
        root: { height: '40px', marginLeft: '5px', background: '#e5e5e5' },
        icon: { color: 'black' },
    };

    const toggleNav = () => {
        setIsNavExpanded(!isNavExpanded);
    };

    const renderNavLinks = () => {
        return isNavExpanded
            ? links.map((linkItem: INavLink) => (
                  <CommandBarButton
                      iconProps={{ iconName: linkItem.icon }}
                      text={linkItem.text}
                      title={linkItem.ariaLabel}
                      href={linkItem.href}
                      styles={navButtonStyles}
                  />
              ))
            : links.map((linkItem: INavLink) => (
                  <IconButton
                      iconProps={{ iconName: linkItem.icon }}
                      styles={navIconStyles}
                      href={linkItem.href}
                      title={linkItem.ariaLabel}
                  />
              ));
    };

    return (
        <Stack
            className="navContainer"
            styles={{
                root: {
                    width: isNavExpanded ? '176px' : '48px',
                    zIndex: isNavExpanded ? '1' : 'auto',
                    height: isMobile ? (isNavExpanded ? '100vh' : '48px') : '100vh',
                },
            }}
        >
            <IconButton
                iconProps={{ iconName: 'GlobalNavButton' }}
                styles={navIconStyles}
                onClick={toggleNav}
                title={(isNavExpanded ? 'Collapse' : 'Expand') + ' Navigation'}
            />

            {(!isMobile || isNavExpanded) && <Stack>{renderNavLinks()}</Stack>}
        </Stack>
    );
}
