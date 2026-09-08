import * as React from 'react';
import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
export default function useAuthClient() {
    const { authClient } = React.useContext(Context as React.Context<IEmployeeExperienceContext>);

    React.useEffect(() => {
        const initializeAuth = async () => {
            await (authClient as any).initializeClient();
        };
        initializeAuth();
    }, [authClient]);

    return true;
}
