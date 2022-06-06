import { Context } from '@micro-frontend-react/employee-experience/lib/Context';
import { IEmployeeExperienceContext } from '@micro-frontend-react/employee-experience/lib/IEmployeeExperienceContext';
import { useContext, useLayoutEffect } from "react";

export const usePersistentReducer = (reducerName: string, reducer: any) => {
    const { reducerRegistry } = useContext(Context as React.Context<IEmployeeExperienceContext>);
    useLayoutEffect(() => {
        if (!reducerRegistry.exists(reducerName)) {
            reducerRegistry.register(reducerName, reducer);
        }
    }, [reducer, reducerName, reducerRegistry]);
}