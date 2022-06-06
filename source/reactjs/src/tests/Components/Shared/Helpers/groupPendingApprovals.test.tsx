import { groupBySubmitter, groupByDate, groupByTenant } from '../../../../Helpers/groupPendingApprovals';

describe('groupPendingApprovals', () => {
    it('should group by submitter work', () => {
        let summary: { appName: string; SubmittedDate: string; Submitter: { Alias: string; Name: string } }[] = [
            { appName: 'Test1', SubmittedDate: '2024-06-20T00:00:00', Submitter: { Alias: 'Btest2', Name: 'Btest2' } },
            { appName: 'Test2', SubmittedDate: '2024-06-19T00:00:00', Submitter: { Alias: 'Atest1', Name: 'Atest1' } },
            { appName: 'Test2', SubmittedDate: '2024-06-19T00:00:00', Submitter: { Alias: 'A1test', Name: 'A1test' } }
        ];
        var group = groupBySubmitter(summary);
        expect(group[0].key).toEqual(summary[2].Submitter.Alias);
        expect(group).toHaveLength(3);
    });

    it('should  group by date work', () => {
        let summary: { appName: string; SubmittedDate: string; Submitter: { Alias: string; Name: string } }[] = [
            { appName: 'Test2', SubmittedDate: '2024-06-20T00:00:00', Submitter: { Alias: 'Atest2', Name: 'Atest2' } },
            { appName: 'Test1', SubmittedDate: '2024-06-19T00:00:00', Submitter: { Alias: 'Btest1', Name: 'Btest1' } }
        ];
        var group = groupByDate(summary);

        expect(group[0].key).toEqual(summary[0].SubmittedDate.substring(0, summary[0].SubmittedDate.indexOf('T')));
        expect(group).toHaveLength(2);
    });

    it('should  group by tenant work', () => {
        let summary: {
            appName: string;
            TenantId: number;
            SubmittedDate: string;
            Submitter: { Alias: string; Name: string };
        }[] = [
            {
                appName: 'Test2',
                TenantId: 21,
                SubmittedDate: '2024-06-20T00:00:00',
                Submitter: { Alias: 'Atest2', Name: 'Atest2' }
            },
            {
                appName: 'Test1',
                TenantId: 31,
                SubmittedDate: '2024-06-19T00:00:00',
                Submitter: { Alias: 'Btest1', Name: 'Btest1' }
            }
        ];
        var group = groupByTenant(summary);

        expect(group[0].key).toEqual(summary[1].TenantId);
        expect(group).toHaveLength(1);
    });
});
