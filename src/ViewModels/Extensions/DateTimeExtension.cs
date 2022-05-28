using System;

namespace ViewModels.Extensions
{
    public static class DateTimeExtension
    {
        public static int Age(this DateTime birthDate)
        {
            return birthDate.Age(DateTime.Today);
        }

        public static int Age(this DateTime birthDate, DateTime now)
        {
            var age = now.Year - birthDate.Year;

            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
            {
                age--;
            }

            return age;
        }
    }
}
