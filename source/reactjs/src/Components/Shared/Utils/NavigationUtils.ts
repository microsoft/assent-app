import { History } from 'history';

export interface INavigationOptions {
    preserveAlias?: boolean;
    preserveDashboardRoute?: boolean;
    clearFilters?: boolean;
}

export interface ISearchNavigationParams {
    searchQuery?: string;
    alias?: string;
    customPath?: string;
}

/**
 * Utility class for handling navigation with consistent URL parameter management
 */
export class NavigationUtils {
    /**
     * Checks if the current route is a dashboard route
     */
    static isDashboardRoute(history: History): boolean {
        return history.location.pathname.includes('/dashboard');
    }

    /**
     * Gets the base path based on current route and options
     */
    static getBasePath(history: History, preserveDashboardRoute = true): string {
        if (preserveDashboardRoute && NavigationUtils.isDashboardRoute(history)) {
            return '/dashboard';
        }
        return '/';
    }

    /**
     * Extracts current URL parameters
     */
    static getCurrentUrlParams(history: History): URLSearchParams {
        return new URLSearchParams(history.location.search);
    }

    /**
     * Builds a new URLSearchParams object with the specified parameters
     */
    static buildUrlParams(
        currentParams: URLSearchParams,
        params: ISearchNavigationParams,
        options: INavigationOptions = {}
    ): URLSearchParams {
        const newUrlSearch = new URLSearchParams();

        // Preserve alias if requested and exists
        if (options.preserveAlias && currentParams.has('alias')) {
            newUrlSearch.set('alias', currentParams.get('alias'));
        }

        // Add custom alias if provided
        if (params.alias) {
            newUrlSearch.set('alias', params.alias);
        }

        // Add search query if provided and not clearing filters
        if (params.searchQuery && !options.clearFilters) {
            newUrlSearch.set('filter', params.searchQuery);
        }

        // Preserve existing filter if not clearing and no new search query
        if (!options.clearFilters && !params.searchQuery && currentParams.has('filter')) {
            newUrlSearch.set('filter', currentParams.get('filter'));
        }

        return newUrlSearch;
    }

    /**
     * Constructs the full navigation path
     */
    static buildNavigationPath(basePath: string, urlParams: URLSearchParams, customPath?: string): string {
        if (customPath) {
            return customPath;
        }

        const searchString = urlParams.toString();
        return searchString ? `${basePath}?${searchString}` : basePath;
    }

    /**
     * Handles navigation for search functionality with consistent parameter management
     */
    static navigateWithSearch(
        history: History,
        params: ISearchNavigationParams,
        options: INavigationOptions = {}
    ): string {
        const currentParams = NavigationUtils.getCurrentUrlParams(history);
        const basePath = NavigationUtils.getBasePath(history, options.preserveDashboardRoute);
        const urlParams = NavigationUtils.buildUrlParams(currentParams, params, options);
        const path = NavigationUtils.buildNavigationPath(basePath, urlParams, params.customPath);

        history.push(path);
        return path;
    }

    /**
     * Helper method specifically for search operations
     */
    static performSearch(
        history: History,
        searchQuery: string,
        options: INavigationOptions = { preserveAlias: true, preserveDashboardRoute: true }
    ): string {
        return NavigationUtils.navigateWithSearch(history, { searchQuery }, options);
    }

    /**
     * Helper method for clearing search filters
     */
    static clearSearch(
        history: History,
        options: INavigationOptions = { preserveAlias: true, preserveDashboardRoute: true }
    ): string {
        return NavigationUtils.navigateWithSearch(history, {}, { ...options, clearFilters: true });
    }

    /**
     * Helper method for delegation navigation
     */
    static navigateWithDelegation(
        history: History,
        alias?: string,
        options: INavigationOptions = { preserveDashboardRoute: true }
    ): string {
        const currentParams = NavigationUtils.getCurrentUrlParams(history);
        const params: ISearchNavigationParams = alias ? { alias } : {};

        // Preserve existing filter when changing delegation
        if (currentParams.has('filter')) {
            params.searchQuery = currentParams.get('filter');
        }

        return NavigationUtils.navigateWithSearch(history, params, options);
    }

    /**
     * Navigate to document details page
     * @param history React Router history object
     * @param tenantId Tenant ID
     * @param displayDocumentNumber Display document number
     * @param preserveAlias Whether to preserve the alias parameter (default: true)
     * @param preserveFilter Whether to preserve the filter parameter (default: true)
     */
    static navigateToDetails(
        history: History,
        tenantId: number | string,
        displayDocumentNumber: string,
        preserveAlias = true,
        preserveFilter = true
    ): void {
        // Check if already on the same document
        const currentPath = history.location.pathname;
        const pathParts = currentPath.split('/');

        // For dashboard routes, check if already on same document (position 3 instead of 2)
        const isDashboard = NavigationUtils.isDashboardRoute(history);
        const documentPosition = isDashboard ? 3 : 2;

        if (pathParts.length > documentPosition && pathParts[documentPosition] === displayDocumentNumber) {
            return; // Already on this document
        }

        // Reuse existing utility methods
        const currentParams = NavigationUtils.getCurrentUrlParams(history);
        const urlParams = NavigationUtils.buildUrlParams(
            currentParams,
            {},
            {
                preserveAlias,
                clearFilters: !preserveFilter,
            }
        );

        // Build the path with tenant and document number, preserving dashboard route if present
        const basePath = isDashboard
            ? `/dashboard/${tenantId}/${displayDocumentNumber}`
            : `/${tenantId}/${displayDocumentNumber}`;
        const path = NavigationUtils.buildNavigationPath(basePath, urlParams);

        // Navigate
        history.push(path);
    }
}
