import { getTenantIcon } from '../../../../Components/Shared/Components/IconMapping';

const defaultTenantIconPath = '/icons/Tenant Default.png';

describe('IconMapping', () => {
    let appName: string = 'Test CELA Invoice';
    let width: string = '26px';
    it('should get default icon', () => {
        let tenantInfo: any = null;
        const wrapper = getTenantIcon(appName, tenantInfo, width);
        expect(wrapper).toBeTruthy();
        expect(wrapper.type).toEqual('img');
        expect(wrapper.props.src).toEqual(defaultTenantIconPath);
        expect(wrapper.props.width).toEqual(width);
    });

    it('should get tenant icon for other than svg image type', () => {
        let tenantInfo: { appName: string; tenantImageDetails: { fileType: any; fileBase64: any } }[] = [
            {
                appName: 'Test CELA Invoice',
                tenantImageDetails: { fileType: 'png', fileBase64: 'iVBORw0KGgoAAAANSUhEUgAAAC' }
            },
            { appName: 'Test1', tenantImageDetails: { fileType: '', fileBase64: '' } }
        ];
        const props = {
            appName: 'Test CELA Invoice',
            width: '26px',
            tenantInfo: tenantInfo
        };
        const wrapper = getTenantIcon(appName, tenantInfo, width);
        expect(wrapper).toBeTruthy();
        expect(wrapper.props.src).toContain(tenantInfo[0].tenantImageDetails.fileType);
        expect(wrapper.type).toEqual('img');
        expect(wrapper.props.width).toEqual(width);
    });

    it('should get tenant icon for svg image type', () => {
        let tenantInfo: { appName: string; tenantImageDetails: { fileType: any; fileBase64: any } }[] = [
            {
                appName: 'Test CELA Invoice',
                tenantImageDetails: { fileType: 'svg', fileBase64: 'iVBORw0KGgoAAAANSUhEUgAAAC' }
            },
            { appName: 'Test1', tenantImageDetails: { fileType: '', fileBase64: '' } }
        ];
        const props = {
            appName: 'Test CELA Invoice',
            width: '26px',
            tenantInfo: tenantInfo
        };
        const wrapper = getTenantIcon(appName, tenantInfo, width);
        expect(wrapper).toBeTruthy();
        expect(wrapper.type).toEqual('img');
        expect(wrapper.props.width).toEqual(width);
    });

    it('should get default icon', () => {
        let tenantInfo: { appName: string; tenantImageDetails: { fileName: string } }[] = [
            {
                appName: 'Test CELA Invoice',
                tenantImageDetails: { fileName: 'Test CELA Invoice' }
            },
            { appName: 'Test1', tenantImageDetails: { fileName: '' } }
        ];
        const props = {
            appName: 'Test',
            width: '26px',
            tenantInfo: tenantInfo
        };
        const wrapper = getTenantIcon(appName, tenantInfo, width);
        expect(wrapper).toBeTruthy();
        expect(wrapper.type).toEqual('img');
        expect(wrapper.props.width).toEqual(width);
    });

    it('should get tenant icon without image details', () => {
        let tenantInfo: { appName: string }[] = [
            {
                appName: 'Test CELA Invoice'
            },
            { appName: 'Test1' }
        ];
        const props = {
            appName: 'Test CELA Invoice',
            width: '26px',
            tenantInfo: tenantInfo
        };
        const wrapper = getTenantIcon(appName, tenantInfo, width);
        expect(wrapper).toBeTruthy();
        expect(wrapper.type).toEqual('img');
        expect(wrapper.props.width).toEqual(width);
    });
});
