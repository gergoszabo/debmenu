export const getDateRange = (start, end) => {
    const startDate = new Date(Date.parse(start) + 3600000);
    const endDate = new Date(Date.parse(end) + 3600000);

    const dates = [];
    for (let d = startDate.getTime(); d < endDate.getTime(); d += 86400000) {
        dates.push(new Date(d));
    }
    dates.push(endDate);

    return dates;
};
