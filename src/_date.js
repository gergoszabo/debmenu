export const getDateRange = (start, end) => {
    const startDate = new Date(Date.parse(start));
    const endDate = new Date(Date.parse(end));

    const dates = [];
    for (let d = startDate.getTime(); d < endDate.getTime(); d += 86400000) {
        dates.push(new Date(d));
    }
    dates.push(endDate);

    return dates;
};
