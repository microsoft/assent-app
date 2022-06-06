export const tableColumns = [
    {
        field: 'status',
        title: 'Approval Status',
        isFilterable: true
    },
    {
        field: 'submittedForFullName',
        title: 'Submitted For',
        isFilterable: true
    },
    {
        field: 'assignmentDetails.assignmentName',
        title: 'Assignment Name',
        isFilterable: true
    },
    {
        field: 'submittedByFullName',
        title: 'Submitted By',
        isFilterable: true
    },
    {
        field: 'assignmentDetails.isBillable',
        title: 'Is Billable',
        isFilterable: true
    },
    {
        field: 'laborNotes',
        title: 'Notes',
        isFilterable: true
    },
    {
        field: 'laborCategoryName',
        title: 'Labor Category',
        isFilterable: true
    },
    {
        field: 'laborDate',
        title: 'Labor Date',
        isFilterable: false,
        isDefaultSortColumn: true
    }
];
