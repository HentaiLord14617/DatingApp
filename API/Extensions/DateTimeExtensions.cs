namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CaculateAge(this DateTime DateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }
            return age;


        }

    }
}