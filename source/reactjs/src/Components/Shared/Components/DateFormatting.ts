export function mapDate(date: string){
    const monthNames = [
        "Jan ", "Feb ", "Mar ",
        "Apr ", "May ", "June ", "July ",
        "Aug ", "Sept ", "Oct ",
        "Nov ", "Dec "
      ];
    var [year, monthIndex, day] = date.substring(0, 10).split("-");
    var month = monthNames[parseInt(monthIndex, 10)-1];
    return month + day.toString() + ", " + year.toString();
}