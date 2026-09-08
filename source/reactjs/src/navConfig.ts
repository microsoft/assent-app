export interface INavLink {
    key: string;
    text: string;
    icon: string;
    href: string;
    ariaLabel: string;
    flightingName?: string;
    secondaryText: string;
    requiresRole?: string[];
}

export const navConfig: INavLink[] = [
    {
        key: 'pendingApprovals',
        text: 'Pending Approvals',
        icon: 'View',
        href: '/',
        ariaLabel: 'Pending Approvals',
        secondaryText: 'Home',
    },
    {
        key: 'history',
        text: 'History',
        icon: 'History',
        href: '/history',
        ariaLabel: 'History',
        secondaryText: 'History',
    },
    {
        key: 'dashboard',
        text: 'Dashboard',
        icon: 'BarChart4',
        href: '/dashboard',
        ariaLabel: 'Dashboard',
        flightingName: 'Dashboard',
        secondaryText: 'Dashboard',
    },
    {
        key: 'admin',
        text: 'Admin',
        icon: 'AdminIcon',
        href: '/admin',
        ariaLabel: 'Admin',
        secondaryText: 'Admin',
        requiresRole: ['Admin'],
    },
];
