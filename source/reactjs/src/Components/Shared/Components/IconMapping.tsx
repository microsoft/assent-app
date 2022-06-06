import React = require("react");
const defaultTenantIconPath = "/icons/Tenant Default.png";

export function getTenantIcon(appName: string, tenantInfo: any, width: string){
    let img = <img src={defaultTenantIconPath} width={width} height="auto" alt="Default Tenant Icon"/>
    if (!tenantInfo){
        return img;
    }
    const appInfo =  tenantInfo.find((tenant: { appName: string; }) => tenant.appName === appName);
    const imageExtension = appInfo?.tenantImageDetails?.fileType;
    const tenantImageBase64 = appInfo?.tenantImageDetails?.fileBase64;
    if (imageExtension && tenantImageBase64){
        let imageBase64Src;
        if (imageExtension === "svg"){
            imageBase64Src = 'data:image/svg+xml;base64,'+tenantImageBase64;
        }
        else{
            imageBase64Src = 'data:image/'+imageExtension+';base64,'+tenantImageBase64;
        }
        img = <img src={imageBase64Src} width={width} height="auto" alt={appInfo.appName + " Icon"} />
    }
    // return default tenant image
    return img;
}