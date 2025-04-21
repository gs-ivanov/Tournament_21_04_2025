namespace Tournament.Data
{
    public class DataConstants
    {
        public class User
        {
            public const int FullNameMinLength = 5;
            public const int FullNameMaxLength = 40;
            public const int PasswordMinLength = 6;
            public const int PasswordMaxLength = 100;
        }

        public class Team
        {
            public const int NameMinLength = 2;
            public const int NameMaxLength = 20;
            public const int CityMinLength = 2;
            public const int CityMaxLength = 30;
            public const int DescriptionMinLength = 5;
            public const int YearMinValue = 1800;
            public const int YearMaxValue = 2050;
        }


    }
}
