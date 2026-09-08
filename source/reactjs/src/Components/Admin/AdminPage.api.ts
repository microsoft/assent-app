import { IHttpClient } from '@micro-frontend-react/employee-experience/lib/IHttpClient';
import { setHeader } from '../Shared/Components/SagasHelper';

function getSessionId(): string {
    return window.sessionStorage.getItem('correlationId') || '';
}

export async function fetchApi<T>(httpClient: IHttpClient, path: string): Promise<T> {
    const url = `${__API_BASE_URL__}${__API_URL_ROOT__}${path}`;
    const { data } = await httpClient.request({
        url,
        resource: __RESOURCE_URL__,
        headers: { ...setHeader(null), 'X-Session-Id': getSessionId() },
    });
    return data as T;
}

export async function patchApi<T>(httpClient: IHttpClient, path: string, body: unknown): Promise<T> {
    const url = `${__API_BASE_URL__}${__API_URL_ROOT__}${path}`;
    const { data } = await httpClient.request({
        url,
        resource: __RESOURCE_URL__,
        headers: { ...setHeader(null), 'X-Session-Id': getSessionId() },
        method: 'PATCH',
        data: body,
    });
    return data as T;
}
