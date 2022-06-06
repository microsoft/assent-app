import { mapDate } from '../Components/Shared/Components/DateFormatting';
import { DATE_FORMAT_OPTION, DEFAULT_LOCALE } from '../Components/Shared/SharedConstants';

export interface IGrouping {
    key: any;  
    displayValue: string;
    grouping: any[];
    isPullModelEnabled?: boolean;
}

const sortCardsByDescendingDate = (grouping: Array<any>): any => {
    return grouping.sort((a, b) => (a.SubmittedDate.substring(0, a.SubmittedDate.indexOf("T")) < b.SubmittedDate.substring(0, b.SubmittedDate.indexOf("T"))) ? 1 : -1);
}

const groupCardsBySubmitter = (grouping: Array<any>): any => {
    return grouping.sort((a, b) => (a.Submitter.Name > b.Submitter.Name) ? 1 : -1);
}

// const sortGroupsByIncreasingAmount = (a: any, b: any): any => {
//     const rangeAStartValue = parseInt(a.match(/^[^\d]*(\d+)/));
//     const rangeBStartValue = parseInt(b.match(/^[^\d]*(\d+)/));
//     return (rangeAStartValue > rangeBStartValue) ? 1 : -1;
// }

export const groupBySubmitter = (summary: object[]): any => {
    const groupedBySubmitter: IGrouping[] = [];

    // create map of names to aliases
    const submittersMap = new Map<string, string>();
    summary.forEach((item: any) => {
        submittersMap.set(item.Submitter.Name, item.Submitter.Alias);
    });

    // grab the approval requests for each alias
    submittersMap.forEach((alias, name) => {
        const submitterGroup: IGrouping = {
            key: alias,
            displayValue: name,
            // sort the cards by descending date in their groups
            grouping: sortCardsByDescendingDate(
                summary.filter((item: any) => {
                    if (item.Submitter.Alias) {
                        return item.Submitter.Alias === alias;
                    } else {
                        return item.Submitter.Name === name;
                    }
                })
            )
        };
        groupedBySubmitter.push(submitterGroup);
    });

    // sort alphabetically by submitter name
    groupedBySubmitter.sort((a, b) => (a.displayValue > b.displayValue) ? 1 : -1);

    return groupedBySubmitter;
}

export const groupByTenant = (summary: object[]): any => {
    const groupedByTenant: IGrouping[] = [];

    // create map of tenant names to tenant ids
    const tenantsMap = new Map<string, number>();
    summary.forEach((item: any) => {
        tenantsMap.set(item.AppName, item.TenantId);
    });

    // grab the approval requests for each alias
    tenantsMap.forEach((tenantId, tenantName) => {
        const tenantGroup :IGrouping = {
            key: tenantId,
            displayValue: tenantName,
            // sort the cards by descending date in their groups
            grouping: sortCardsByDescendingDate(summary.filter((item: object) => (item as any).TenantId === tenantId))
        }
        groupedByTenant.push(tenantGroup);
    });

    // sort alphabetically by submitter name
    groupedByTenant.sort((a, b) => (a.displayValue > b.displayValue) ? 1 : -1);

    return groupedByTenant;
}

export const groupByDate = (summary: object[]): any => {
    const groupedByDate: IGrouping[] = [];
    
    // create set of dates
    const datesSet = new Set<string>();

    //  //removing timestamp from submitted date
    //  summary = summary.map((item:any) => ({
    //     ...item,
    //     SubmittedDate: item.SubmittedDate.replace(/T.+/, '')
    // }));

    summary.forEach((item: any) => {
        // get the local date without the time stamp
        const formattedDate = new Date(item.SubmittedDate).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION)
        datesSet.add(formattedDate)
    });

    // grab the approval requests for each date
    datesSet.forEach((date) => {
        var dateGroup: IGrouping = {
            key: new Date(date),
            displayValue: date,
            // sort the cards by submitter name in their groups
            grouping: groupCardsBySubmitter(summary.filter((item: any) => new Date(item.SubmittedDate).toLocaleDateString(DEFAULT_LOCALE, DATE_FORMAT_OPTION) === date))
        }
        groupedByDate.push(dateGroup);
    });

    // sort by descending date
    groupedByDate.sort((a, b) => b.key - a.key);

    return groupedByDate;
}

// export const groupByAmount = (summary: object[]) => {
//     // display value - the amount range (same as key?)
//     const groupedByAmount: IGrouping[] = [];

//     // create map of amounts and their groupings
//     let amountsMap = new Map<string, []>();
//     summary.forEach((item: any) => {
//         // if key doesn't exist, add it
//         let amountRange = getAmountRange(item.UnitValue);
//         if(!amountsMap.has(amountRange)) {
//             amountsMap.set(amountRange, []);
//         }
//         amountsMap.get(amountRange).push(item);
//     });

//     // convert to an array of IGrouping objects
//     amountsMap.forEach((amountMapGroup, amountRange) => {
//         var amountGroup: IGrouping = {
//             key: amountRange,
//             displayValue: amountRange,
//             // sort the cards by descending date in their groups
//             grouping: groupCardsByDescendingDate(amountMapGroup)
//         }
//         groupedByAmount.push(amountGroup);
//     });

//     // sort by range value
//     groupedByAmount.sort(sortGroupsByIncreasingAmount);

//     return groupedByAmount;

// }

// const amountRanges = {
//     LESSTHANTEN: '0-10',
//     LESSTHANHUNDRED: '11-100',
//     LESSTHANTHOUSAND: '101-1000',
//     LESSTHANTENTHOUSAND: '1001-10000',
//     LESSTHANHUNDREDTHOUSAND: '10001-100000',
//     LESSTHANMILLION: '100001-1000000',
//     GREATERTHANMILLION: '> 100000'
// }

// const getAmountRange = (value: any) => {
//     if (value >= 0 && value <= 10)
//         return amountRanges.LESSTHANTEN;
//     else if (value >= 11 && value <= 100)
//         return amountRanges.LESSTHANHUNDRED;
//     else if (value >= 101 && value <= 1000)
//         return amountRanges.LESSTHANTHOUSAND;
//     else if (value >= 1001 && value <= 10000)
//         return amountRanges.LESSTHANTENTHOUSAND;
//     else if (value >= 10001 && value <= 100000)
//         return amountRanges.LESSTHANHUNDREDTHOUSAND;
//     else if (value >= 100001 && value <= 1000000)
//         return amountRanges.LESSTHANMILLION;
//     else
//         return amountRanges.GREATERTHANMILLION;
// };
